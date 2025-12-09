using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchScrapingResultsUpdated;

public record WatchdogSearchScrapingResultsUpdatedDomainEvent(long WatchdogSearchId) : DomainEvent;