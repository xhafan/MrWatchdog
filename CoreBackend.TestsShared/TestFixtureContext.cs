using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.Configurations;
using CoreUtils.AmbientStorages;

namespace CoreBackend.TestsShared;

public static class TestFixtureContext
{
    public static INhibernateConfigurator NhibernateConfigurator = null!;
    public static readonly AmbientStorage<ICollection<IDomainEvent>?> RaisedDomainEvents = new();
    public static ICollection<Type>? AggregateRootEntityTypes;
}