using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record MakeWatchdogPublicCommand(long WatchdogId) : Command;