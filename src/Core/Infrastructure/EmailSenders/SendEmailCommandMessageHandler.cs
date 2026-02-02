using Microsoft.Extensions.Hosting;
using Rebus.Handlers;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SendEmailCommandMessageHandler(
    IEmailSender emailSender,
    IHostEnvironment environment
) : IHandleMessages<SendEmailCommand>
{
    public async Task Handle(SendEmailCommand command)
    {
        var subject = environment.IsProduction() 
            ? command.Subject
            : $"({environment.EnvironmentName}) {command.Subject}";
        await emailSender.SendEmail(
            command.RecipientEmail,
            subject,
            command.HtmlMessage,
            command.UnsubscribeUrl
        );
    }
}