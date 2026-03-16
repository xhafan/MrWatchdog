using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Rebus.MessageRouting;
using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

[RebusRouting(RebusQueues.Scraping)]
public record ScrapeScraperCommand(long ScraperId) : Command;