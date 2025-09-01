using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;

public record WatchdogWebPageScrapingDataUpdatedDomainEvent(long WatchdogId, long WatchdogWebPageId) : DomainEvent;
