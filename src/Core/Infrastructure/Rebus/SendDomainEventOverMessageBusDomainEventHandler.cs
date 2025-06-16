using CoreDdd.Domain.Events;
using CoreUtils;
using MrWatchdog.Core.Messages;
using Rebus.Bus.Advanced;

namespace MrWatchdog.Core.Infrastructure.Rebus;

/// <summary>
/// Handles all domain events by sending them over a message bus.
/// </summary>
/// <typeparam name="TDomainEvent">Given domain event type</typeparam>
public class SendDomainEventOverMessageBusDomainEventHandler<TDomainEvent>(ISyncBus bus)
    : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : DomainEvent
{
    public void Handle(TDomainEvent domainEvent)
    {
        domainEvent.RelatedCommandGuid = JobContext.CommandGuid.Value;

        Guard.Hope(JobContext.RaisedDomainEvents.Value != null, $"{nameof(JobContext)} {nameof(JobContext.RaisedDomainEvents)} is null");
        
        if (JobContext.RaisedDomainEvents.Value.Contains(domainEvent))
        {
            return;
        }
        
        
        bus.Send(domainEvent);
        
        JobContext.RaisedDomainEvents.Value.Add(domainEvent);
    }
}
