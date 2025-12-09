using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

[RebusRouting(RebusQueues.Scraping)]
public record ScrapeWatchdogCommand(long WatchdogId) : Command;