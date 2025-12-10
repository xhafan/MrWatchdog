using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record RefreshWatchdogCommand(long WatchdogId) : Command;