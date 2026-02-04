using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingResultsUpdated;

public record WatchdogScrapingResultsUpdatedDomainEvent(long WatchdogId) : DomainEvent;