using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record ScrapeWatchdogWebPageCommand(long WatchdogId, long WatchdogWebPageId) : Command;