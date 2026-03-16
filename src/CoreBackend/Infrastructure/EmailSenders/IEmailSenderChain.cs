namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public interface IEmailSenderChain
{
    Task SendEmail(
        string recipientEmail, 
        string subject, 
        string htmlMessage, 
        string? unsubscribeUrl = null
    );
}