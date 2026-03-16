using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;

public record ScraperScrapingCompletedDomainEvent(long ScraperId) : DomainEvent;