using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;

public record WatchdogAlertScrapingResultsUpdatedDomainEvent(long WatchdogAlertId) : DomainEvent;