using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;

public record WatchdogScrapingCompletedDomainEvent(long WatchdogId) : DomainEvent;