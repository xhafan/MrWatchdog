namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public interface IEmailSender
{
    Task SendEmail(
        string recipientEmail,
        string subject,
        string htmlMessage,
        string? unsubscribeUrl = null
    );
}