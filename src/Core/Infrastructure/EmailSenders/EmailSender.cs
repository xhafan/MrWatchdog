using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class EmailSender(IOptions<EmailSenderOptions> options) : IEmailSender
{
    public async Task SendEmail(string recipientEmail, string subject, string htmlMessage)
    {
        var emailSenderOptions = options.Value;
        using var client = new SmtpClient(emailSenderOptions.SmtpServer, emailSenderOptions.Port);
        client.Credentials = new NetworkCredential(emailSenderOptions.Username, emailSenderOptions.Password);
        client.EnableSsl = true;
        
        var message = new MailMessage(emailSenderOptions.FromAddress, recipientEmail, subject, htmlMessage)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(message);
    }
}