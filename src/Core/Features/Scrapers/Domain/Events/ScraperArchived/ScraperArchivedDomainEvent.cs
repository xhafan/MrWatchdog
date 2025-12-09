using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperArchived;

public record ScraperArchivedDomainEvent(long ScraperId) : DomainEvent;