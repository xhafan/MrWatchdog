using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;
using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

[RebusRouting(RebusQueues.Scraping)]
public record ScrapeScraperCommand(long ScraperId) : Command;