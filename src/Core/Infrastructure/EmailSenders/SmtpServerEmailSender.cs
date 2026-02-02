using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SmtpServerEmailSender(
    IOptions<SmtpServerEmailSenderOptions> iEmailSenderOptions,
    IOptions<EmailAddressesOptions> iEmailAddressesOptions
) : IEmailSender
{
    public async Task SendEmail(
        string recipientEmail, 
        string subject, 
        string htmlMessage, 
        string? unsubscribeUrl = null
    )
    {
        var emailSenderOptions = iEmailSenderOptions.Value;
        using var client = new SmtpClient(emailSenderOptions.SmtpServer, emailSenderOptions.Port);
        client.Credentials = new NetworkCredential(emailSenderOptions.Username, emailSenderOptions.Password);
        client.EnableSsl = true;
        
        var message = new MailMessage(iEmailAddressesOptions.Value.NoReply, recipientEmail, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        if (!string.IsNullOrWhiteSpace(unsubscribeUrl))
        {
            // make sure the SMTP server supports these headers; for instance mailersend.net supports them only in Professional or Enterprise plan
            message.Headers.Add("List-Unsubscribe", $"<{unsubscribeUrl}>");
            message.Headers.Add("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");
        }

        await client.SendMailAsync(message);
    }
}