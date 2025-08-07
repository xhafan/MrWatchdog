using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public record ScrapeWatchdogCommand(long WatchdogId) : Command;