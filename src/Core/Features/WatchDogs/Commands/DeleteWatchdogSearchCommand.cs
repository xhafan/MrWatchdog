using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record DeleteWatchdogSearchCommand(long WatchdogSearchId) : Command;