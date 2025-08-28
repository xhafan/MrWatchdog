using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record MarkLoginTokenAsUsedCommand(Guid LoginTokenGuid) : Command;