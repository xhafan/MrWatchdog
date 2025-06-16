using Castle.Windsor;
using CoreDdd.Domain.Events;
using CoreUtils;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class DomainEventHandlerFactory : IDomainEventHandlerFactory
{
    public IEnumerable<IDomainEventHandler<TDomainEvent>> Create<TDomainEvent>() where TDomainEvent : IDomainEvent
    {
        var windsorContainer = _GetJobContextWindsorContainer();
        return windsorContainer.ResolveAll<IDomainEventHandler<TDomainEvent>>();
    }

    public void Release(object domainEventHandler)
    {
        var windsorContainer = _GetJobContextWindsorContainer();
        windsorContainer.Release(domainEventHandler);
    }
    
    private IWindsorContainer _GetJobContextWindsorContainer()
    {
        var windsorContainer = JobContext.WindsorContainer.Value;
        Guard.Hope(windsorContainer != null, $"{nameof(JobContext)}.{nameof(windsorContainer)} is null");
        return windsorContainer;
    }
    
}