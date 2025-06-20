using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events;

public record WatchdogWebPageScrapingDataUpdatedDomainEvent(long WatchdogId, long WatchdogWebPageId) : DomainEvent;
