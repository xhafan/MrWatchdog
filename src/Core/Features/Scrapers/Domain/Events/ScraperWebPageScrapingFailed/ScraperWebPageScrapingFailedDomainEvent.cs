using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;

public record WatchdogWebPageScrapingFailedDomainEvent(long WatchdogId) : DomainEvent;
