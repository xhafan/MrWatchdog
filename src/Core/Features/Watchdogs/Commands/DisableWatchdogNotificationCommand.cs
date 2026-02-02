using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record DisableWatchdogNotificationCommand(long WatchdogId) : Command;