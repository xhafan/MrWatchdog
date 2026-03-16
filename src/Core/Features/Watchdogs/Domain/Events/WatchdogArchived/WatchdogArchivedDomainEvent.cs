using CoreBackend.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;

public record WatchdogArchivedDomainEvent(long WatchdogId) : DomainEvent;