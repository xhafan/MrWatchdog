using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;

public record ScraperWebPageScrapingFailedDomainEvent(long ScraperId) : DomainEvent;
