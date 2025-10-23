using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpServerEmailSender(
    IOptions<SmtpServerEmailSenderOptions> iEmailSenderOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
) : IEmailSender
{
    public async Task SendEmail(string recipientEmail, string subject, string htmlMessage)
    {
        var emailSenderOptions = iEmailSenderOptions.Value;
        using var client = new SmtpClient(emailSenderOptions.SmtpServer, emailSenderOptions.Port);
        client.Credentials = new NetworkCredential(emailSenderOptions.Username, emailSenderOptions.Password);
        client.EnableSsl = true;
        
        var message = new MailMessage(iEmailAddressesOptions.Value.NoReply, recipientEmail, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }
}