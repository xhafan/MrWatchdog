using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchArchived;

public record WatchdogSearchArchivedDomainEvent(long WatchdogSearchId) : DomainEvent;