using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.MessageRouting;
using CoreBackend.Messages;

namespace CoreBackend.Infrastructure.EmailSenders;

[RebusRouting(RebusQueues.Email)]
public record SendEmailCommand(
    string RecipientEmail, 
    string Subject, 
    string HtmlMessage,
    string? UnsubscribeUrl = null
) : Command;
