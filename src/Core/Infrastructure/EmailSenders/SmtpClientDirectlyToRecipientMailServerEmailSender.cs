using CoreUtils;
using DnsClient;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Cryptography;
using System.Text;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpClientDirectlyToRecipientMailServerEmailSender(
    IOptions<SmtpClientDirectlyToRecipientMailServerEmailSenderOptions> iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions,
    ILogger<SmtpClientDirectlyToRecipientMailServerEmailSender>? logger = null
) : IEmailSender
{
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
        
        using var smtp = new SmtpClient();
        var ehloDomainName = iSmtpClientDirectlyToRecipientMailServerEmailSenderOptions.Value.EhloDomainName;
        if (!string.IsNullOrWhiteSpace(ehloDomainName))
        {
            logger?.LogInformation("Setting EHLO domain name: {ehloDomainName}", ehloDomainName);
            smtp.LocalDomain = ehloDomainName;
        }

        logger?.LogInformation("Mail server: {mailServer}", mailServer);

        await smtp.ConnectAsync(mailServer, 25, SecureSocketOptions.StartTlsWhenAvailable);
        
        await smtp.SendAsync(message);
        await smtp.DisconnectAsync(true);
    }
}