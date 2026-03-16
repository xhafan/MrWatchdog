using CoreDdd.Domain.Events;
using CoreIoC;
using CoreUtils.AmbientStorages;

namespace CoreBackend.Infrastructure.Rebus;

public static class JobContext
{
    public static readonly AmbientStorage<List<(string EntityName, long EntityId, bool IsCreated)>?> AffectedEntities = new();
    public static readonly AmbientStorage<IContainer?> IoCContainer = new();
    public static readonly AmbientStorage<HashSet<IDomainEvent>?> RaisedDomainEvents = new();
    public static readonly AmbientStorage<Guid> CommandGuid = new();
    public static readonly AmbientStorage<long> ActingUserId = new();
    public static readonly AmbientStorage<string?> RequestId = new();
    public static readonly AmbientStorage<string?> RebusHandlingQueue = new();
}