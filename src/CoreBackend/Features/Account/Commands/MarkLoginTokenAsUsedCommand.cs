using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record MarkLoginTokenAsUsedCommand(Guid LoginTokenGuid) : Command;