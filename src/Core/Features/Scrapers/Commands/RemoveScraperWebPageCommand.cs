using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record RemoveWatchdogWebPageCommand(long WatchdogId, long WatchdogWebPageId) : Command;