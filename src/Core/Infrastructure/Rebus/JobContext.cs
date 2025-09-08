using Castle.Windsor;
using CoreDdd.Domain.Events;
using CoreUtils.AmbientStorages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public static class JobContext
{
    public static readonly AmbientStorage<List<(string EntityName, long EntityId, bool IsCreated)>?> AffectedEntities = new();
    public static readonly AmbientStorage<IWindsorContainer?> WindsorContainer = new();
    public static readonly AmbientStorage<HashSet<IDomainEvent>?> RaisedDomainEvents = new();
    public static readonly AmbientStorage<Guid> CommandGuid = new();
    public static readonly AmbientStorage<long> ActingUserId = new();
    public static readonly AmbientStorage<string?> RequestId = new();
}