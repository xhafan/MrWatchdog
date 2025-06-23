using CoreDdd.Domain.Events;
using CoreUtils;
using MrWatchdog.Core.Messages;
using Rebus.Bus.Advanced;
using Rebus.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

/// <summary>
/// Handles all domain events by sending them over a message bus.
/// </summary>
/// <typeparam name="TDomainEvent">Given domain event type</typeparam>
public class SendDomainEventOverMessageBusDomainEventHandler<TDomainEvent>(ISyncBus bus, IJobCreator jobCreator)
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
        
        var domainEventMessageGuid = Guid.NewGuid();
        var headers = new Dictionary<string, string>
        {
            [Headers.MessageId] = domainEventMessageGuid.ToString()
        };
        
        AsyncHelper.RunSync(() => jobCreator.CreateJob(domainEvent, domainEventMessageGuid, shouldMarkJobAsHandlingStarted: false));
        
        bus.Send(domainEvent, headers);
        
        JobContext.RaisedDomainEvents.Value.Add(domainEvent);
    }
}
