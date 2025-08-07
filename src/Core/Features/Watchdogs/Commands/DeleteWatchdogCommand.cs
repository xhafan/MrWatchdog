using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record DeleteWatchdogCommand(long WatchdogId) : Command;