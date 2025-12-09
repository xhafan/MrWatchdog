using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogSearchArchived;

public record WatchdogSearchArchivedDomainEvent(long WatchdogSearchId) : DomainEvent;