using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class JobAggregateRootEntity : VersionedEntity
{
    protected JobAggregateRootEntity() {}

    public JobAggregateRootEntity(
        Job job,
        string aggregateRootEntityName,
        long aggregateRootEntityId
    )
    {
        Job = job;
        AggregateRootEntityName = aggregateRootEntityName;
        AggregateRootEntityId = aggregateRootEntityId;
    }

    public virtual Job Job { get; } = null!;
    public virtual string AggregateRootEntityName { get; } = null!;
    public virtual long AggregateRootEntityId { get; }

    public virtual JobAggregateRootEntityDto GetDto()
    {
        return new JobAggregateRootEntityDto(AggregateRootEntityName, AggregateRootEntityId);
    }
}