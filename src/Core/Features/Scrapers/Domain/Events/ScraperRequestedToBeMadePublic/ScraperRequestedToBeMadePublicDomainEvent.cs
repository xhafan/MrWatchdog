using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogRequestedToBeMadePublic;

public record WatchdogRequestedToBeMadePublicDomainEvent(long WatchdogId) : DomainEvent;