using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Infrastructure.EmailSenders;

[RebusRouting(RebusQueues.Email)]
public record SendEmailCommand(
    string RecipientEmail, 
    string Subject, 
    string HtmlMessage,
    string? UnsubscribeUrl = null
) : Command;
