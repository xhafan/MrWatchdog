using CoreDdd.Domain.Events;
using CoreUtils;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;
using MrWatchdog.Core.Messages;
using Rebus.Bus.Advanced;
using Rebus.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

/// <summary>
/// Handles all domain events by sending them over a message bus.
/// </summary>
/// <typeparam name="TDomainEvent">Given domain event type</typeparam>
public class SendDomainEventOverMessageBusDomainEventHandler<TDomainEvent>(
    ISyncBus bus, 
    IJobCreator jobCreator,
    IRebusQueueRedirector rebusQueueRedirector
)
    : IDomainEventHandler<TDomainEvent> where TDomainEvent : DomainEvent
{
    public void Handle(TDomainEvent domainEvent)
    {
        domainEvent.RelatedCommandGuid = JobContext.CommandGuid.Value;
        domainEvent.ActingUserId = JobContext.ActingUserId.Value;
        domainEvent.RequestId = JobContext.RequestId.Value;

        Guard.Hope(JobContext.RaisedDomainEvents.Value != null, $"{nameof(JobContext)} {nameof(JobContext.RaisedDomainEvents)} is null");
        
        if (JobContext.RaisedDomainEvents.Value.Contains(domainEvent))
        {
            return;
        }
        
        var domainEventMessageGuid = Guid.NewGuid();

        var queueForRedirection = rebusQueueRedirector.GetQueueForRedirection();
        Guard.Hope(queueForRedirection != null, "Queue for redirection is not set.");

        var headers = new Dictionary<string, string>
        {
            {Headers.MessageId, domainEventMessageGuid.ToString()},
            {CustomHeaders.QueueForRedirection, queueForRedirection}
        };
        
        AsyncHelper.RunSync(() => jobCreator.CreateJob(domainEvent, domainEventMessageGuid, shouldMarkJobAsHandlingStarted: false));
        
        bus.Send(domainEvent, headers);
        
        JobContext.RaisedDomainEvents.Value.Add(domainEvent);
    }
}
