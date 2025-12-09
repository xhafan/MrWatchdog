using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record RefreshWatchdogSearchCommand(long WatchdogSearchId) : Command;