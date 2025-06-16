using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events;

public record WatchdogWebPageUpdatedDomainEvent(long WatchdogId, long WatchdogWebPageId) : DomainEvent;
