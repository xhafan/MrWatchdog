using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;

public record ScraperRequestedToBeMadePublicDomainEvent(long ScraperId) : DomainEvent;