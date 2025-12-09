using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;

public record ScraperWebPageScrapingDataUpdatedDomainEvent(long ScraperId, long ScraperWebPageId) : DomainEvent;
