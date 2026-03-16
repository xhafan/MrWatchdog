using Microsoft.Extensions.Logging;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class EmailSenderChain(
    IEnumerable<IEmailSender> emailSenders,
    ILogger<EmailSenderChain>? logger = null
) : IEmailSenderChain
{
    public async Task SendEmail(
        string recipientEmail, 
        string subject, 
        string htmlMessage, 
        string? unsubscribeUrl = null
    )
    {
        var emailSendersOrderedByPriority = emailSenders
            .OrderBy(s => s.Priority)
            .ToList();

        var failureReasons = new List<string>();

        foreach (var emailSender in emailSendersOrderedByPriority)
        {
            try
            {
                await emailSender.SendEmail(
                    recipientEmail,
                    subject,
                    htmlMessage,
                    unsubscribeUrl
                );
                logger?.LogInformation("{emailSenderType} successfully sent email: {recipientEmail}, subject {subject}", emailSender.GetType().Name, recipientEmail, subject);
                return;
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "{emailSenderType} failed sending email: {recipientEmail}, subject {subject}", emailSender.GetType().Name, recipientEmail, subject);
                failureReasons.Add($"{emailSender.GetType().Name}: {ex.Message}");
            }
        }

        throw new Exception($"Could not send email:\n{string.Join("\n", failureReasons)}");
    }
}