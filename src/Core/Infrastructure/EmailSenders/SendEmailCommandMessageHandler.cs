using Microsoft.Extensions.Hosting;
using Rebus.Handlers;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

public class SendEmailCommandMessageHandler(
    IEmailSenderChain emailSenderChain,
    IHostEnvironment environment
) : IHandleMessages<SendEmailCommand>
{
    public async Task Handle(SendEmailCommand command)
    {
        var subject = environment.IsProduction() 
            ? command.Subject
            : $"({environment.EnvironmentName}) {command.Subject}";
        await emailSenderChain.SendEmail(
            command.RecipientEmail,
            subject,
            command.HtmlMessage,
            command.UnsubscribeUrl
        );
    }
}