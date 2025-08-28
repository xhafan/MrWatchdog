using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Account.Commands;

public record ConfirmLoginTokenCommand(Guid LoginTokenGuid) : Command;