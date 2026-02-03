using MailKit;
using MailKit.Net.Smtp;
using MimeKit;

namespace MrWatchdog.Core.Tests.Infrastructure.EmailSenders.SmtpClientDirectlyToRecipientMailServerEmailSenders;

public class FakeRateLimitingSmtpClient : SmtpClient
{
    public override Task<string> SendAsync(
        MimeMessage message, 
        CancellationToken cancellationToken = new(),
        ITransferProgress? progress = null
    )
    {
        throw new SmtpCommandException(
            SmtpErrorCode.MessageNotAccepted,
            SmtpStatusCode.ServiceNotAvailable,
            """
            4.7.28 Gmail has detected an unusual rate of unsolicited mail originating
            4.7.28 from your IP Netblock [46.224.68.58      34]. To protect our users
            4.7.28 from spam, mail sent from your IP Netblock has been temporarily rate
            4.7.28 limited. For more information, go to
            4.7.28  https://support.google.com/mail/?p=UnsolicitedRateLimitError to
            4.7.28 review our Bulk Email Senders Guidelines. ffacd0b85a97d-4324eab2651si10575209f8f.183 - gsmtp
            """
        );
    }
}