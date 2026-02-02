namespace MrWatchdog.Core.Infrastructure.EmailSenders;

// For testing purposes
public class NullEmailSender : IEmailSender
{
    public Task SendEmail(
        string recipientEmail, 
        string subject, 
        string htmlMessage, 
        string? unsubscribeUrl = null
    )
    {
        return Task.CompletedTask;
    }
}