using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events;

public record WatchdogScrapingCompletedDomainEvent(long WatchdogId) : DomainEvent;