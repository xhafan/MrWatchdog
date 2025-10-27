using Rebus.Handlers;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SendEmailCommandMessageHandler(IEmailSender emailSender) : IHandleMessages<SendEmailCommand>
{
    public async Task Handle(SendEmailCommand command)
    {
        await emailSender.SendEmail(command.RecipientEmail, command.Subject, command.HtmlMessage);
    }
}