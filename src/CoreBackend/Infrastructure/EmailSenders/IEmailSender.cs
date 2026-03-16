namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public interface IEmailSender
{
    int Priority { get; }

    Task SendEmail(
        string recipientEmail,
        string subject,
        string htmlMessage,
        string? unsubscribeUrl = null
    );
}