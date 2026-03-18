using CoreBackend.Infrastructure.Rebus.MessageRouting;
using CoreBackend.Messages;
using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

[RebusRouting(CustomRebusQueues.Scraping)]
public record ScrapeScraperCommand(long ScraperId) : Command;