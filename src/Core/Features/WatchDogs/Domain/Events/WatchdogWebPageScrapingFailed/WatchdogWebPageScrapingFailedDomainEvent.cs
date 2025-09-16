using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.WatchDogs.Domain.Events.WatchdogWebPageScrapingFailed;

public record WatchdogWebPageScrapingFailedDomainEvent(long WatchdogId) : DomainEvent;
