using CoreDdd.Domain.Events;
using CoreIoC;
using CoreUtils;

namespace CoreBackend.Infrastructure.Rebus;

public class DomainEventHandlerFactory : IDomainEventHandlerFactory
{
    public IEnumerable<IDomainEventHandler<TDomainEvent>> Create<TDomainEvent>() where TDomainEvent : IDomainEvent
    {
        var ioCContainer = _GetJobContextIoCContainer();
        return ioCContainer.ResolveAll<IDomainEventHandler<TDomainEvent>>();
    }

    public void Release(object domainEventHandler)
    {
        var ioCContainer = _GetJobContextIoCContainer();
        ioCContainer.Release(domainEventHandler);
    }
    
    private IContainer _GetJobContextIoCContainer()
    {
        var ioCContainer = JobContext.IoCContainer.Value.Value;
        Guard.Hope(ioCContainer != null, $"{nameof(JobContext)}.{nameof(JobContext.IoCContainer)} is null");
        return ioCContainer;
    }
    
}