using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;
using DnsClient;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Cryptography;
using MrWatchdog.Core.Infrastructure.EmailSenders.Domain;
using System.Net.Security;
using System.Text;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpClientDirectlyToRecipientMailServerEmailSender(
    IOptions<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions> iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions,
    INhibernateConfigurator nhibernateConfigurator,
    ILogger<SmtpClientDirectlyToRecipientMailServerEmailSender>? logger = null,
    Func<SmtpClient>? smtpClientFactory = null
) : IEmailSender
{
    public int Priority => 10;

    public async Task SendEmail(
        string recipientEmail, 
        string subject, 
        string htmlMessage, 
        string? unsubscribeUrl = null
    )
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(name: null, iEmailAddressesOptions.Value.NoReply));
        message.To.Add(new MailboxAddress(name: null, recipientEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlMessage };

        if (!string.IsNullOrWhiteSpace(unsubscribeUrl))
        {
            message.Headers.Add(HeaderId.ListUnsubscribe, $"<{unsubscribeUrl}>");
            message.Headers.Add(HeaderId.ListUnsubscribePost, "List-Unsubscribe=One-Click");
        }

        var signer = new DkimSigner(
            new MemoryStream(Encoding.UTF8.GetBytes(iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions.Value.DkimPrivateKey)),
            iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions.Value.DkimDomain,
            iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions.Value.DkimSelector
        )
        {
            HeaderCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed,
            BodyCanonicalizationAlgorithm = DkimCanonicalizationAlgorithm.Relaxed
        };

        var headersToSign = new List<HeaderId> {
            HeaderId.From,
            HeaderId.To,
            HeaderId.Subject,
            HeaderId.Date,
            HeaderId.MessageId
        };

        if (!string.IsNullOrWhiteSpace(unsubscribeUrl))
        {
            headersToSign.Add(HeaderId.ListUnsubscribe);
            headersToSign.Add(HeaderId.ListUnsubscribePost);
        }

        message.Prepare(EncodingConstraint.SevenBit);
        signer.Sign(message, headersToSign);

        var lookup = new LookupClient();
        var recipientEmailDomain = message.To.Mailboxes.First().Address.Split('@')[1];
        var mxRecords = (
                await lookup.QueryAsync(recipientEmailDomain, QueryType.MX)
            )
            .Answers.MxRecords()
            .OrderBy(mx => mx.Preference)
            .ToList();

        Guard.Hope(mxRecords.Any(), $"No MX record found for domain {recipientEmailDomain}.");
        var mailServer = mxRecords.First().Exchange.Value;
        
        logger?.LogInformation("Mail server: {mailServer}", mailServer);

        if (await _IsMailServerIsInCoolDownPeriodDueToRateLimiting(mailServer))
        {
            throw new EmailSendingNotAllowedInCoolDownPeriodException(
                $"Email not sent due to {mailServer} mail server being in cool down period due to the previous email being rate limited."
            );
        }

        using var smtpClient = smtpClientFactory?.Invoke() ?? new SmtpClient();
        var ehloDomainName = iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions.Value.EhloDomainName;
        if (!string.IsNullOrWhiteSpace(ehloDomainName))
        {
            logger?.LogInformation("Setting EHLO domain name: {ehloDomainName}", ehloDomainName);
            smtpClient.LocalDomain = ehloDomainName;
        }

        _allowConnectionEvenIfCertificateValidationFails();

        await smtpClient.ConnectAsync(mailServer, 25, SecureSocketOptions.StartTlsWhenAvailable);

        try
        {
            await smtpClient.SendAsync(message);
        }
        catch (SmtpCommandException ex) when (ex.Message.Contains("4.7.28") 
                                              && ex.Message.Contains("rate") 
                                              && ex.Message.Contains("limited"))
        {
            await _SetMailServerCoolDownPeriodDueToRateLimiting(mailServer);

            throw new EmailSendingRateLimitedException($"Email not sent due to rate limiting by {mailServer} mail server.", ex);
        }

        await smtpClient.DisconnectAsync(true);
        return;

        void _allowConnectionEvenIfCertificateValidationFails()
        {
            smtpClient.ServerCertificateValidationCallback = (_, _, _, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None) return true;

                logger?.LogWarning("SSL Certificate for {mailServer} has policy errors: {errors}", mailServer, sslPolicyErrors);

                return true; // In a Direct-to-MX scenario, return true to ensure delivery.
            };
        }
    }

    private async Task _SetMailServerCoolDownPeriodDueToRateLimiting(string mailServer)
    {
        try
        {
            await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(nhibernateConfigurator),
                async newUnitOfWork =>
                {
                    var mailServerRateLimiting = await newUnitOfWork.Session!.QueryOver<MailServerRateLimiting>()
                        .Where(x => x.MailServerName == mailServer)
                        .SingleOrDefaultAsync();
                    if (mailServerRateLimiting == null)
                    {
                        mailServerRateLimiting = new MailServerRateLimiting(mailServer, DateTime.UtcNow);
                        await newUnitOfWork.Session.SaveAsync(mailServerRateLimiting);
                    }
                    else
                    {
                        mailServerRateLimiting.SetLastRateLimitedOn(DateTime.UtcNow);
                    }
                }
            );
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "_SetMailServerCoolDownPeriodDueToRateLimiting: {exceptionMessage}", ex.Message);
        }
    }

    private async Task<bool> _IsMailServerIsInCoolDownPeriodDueToRateLimiting(string mailServer)
    {
        var mailServerIsInCoolDownPeriod = false;

        try
        {
            await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(nhibernateConfigurator),
                async newUnitOfWork =>
                {
                    var mailServerRateLimiting = await newUnitOfWork.Session!.QueryOver<MailServerRateLimiting>()
                        .Where(x => x.MailServerName == mailServer)
                        .SingleOrDefaultAsync();
                    
                    if (mailServerRateLimiting == null) return;

                    var coolDownPeriodEnd = mailServerRateLimiting.LastRateLimitedOn
                        .AddMinutes(iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions.Value.MailServerRateLimitingCoolDownPeriodInMinutes);

                    mailServerIsInCoolDownPeriod = DateTime.UtcNow <= coolDownPeriodEnd;
                }
            );
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "_IsMailServerIsInCoolDownPeriodDueToRateLimiting: {exceptionMessage}", ex.Message);
        }

        return mailServerIsInCoolDownPeriod;
    }
}