using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;

public record WatchdogScrapedResultsUpdatedDomainEvent(long WatchdogId) : DomainEvent;