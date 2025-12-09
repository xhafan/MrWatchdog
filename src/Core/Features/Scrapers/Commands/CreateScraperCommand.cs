using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record CreateWatchdogCommand(long UserId, string Name) : Command;