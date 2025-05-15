using CoreDdd.Domain.Events;
using CoreUtils.AmbientStorages;
using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.TestsShared;

public static class TestFixtureContext
{
    public static NhibernateConfigurator? NhibernateConfigurator;
    public static readonly AmbientStorage<ICollection<IDomainEvent>?> RaisedDomainEvents = new();
}