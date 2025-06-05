using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class JobAffectedEntity : VersionedEntity
{
    protected JobAffectedEntity() {}

    public JobAffectedEntity(
        Job job,
        string entityName,
        long entityId,
        bool isCreated
    )
    {
        Job = job;
        EntityName = entityName;
        EntityId = entityId;
        IsCreated = isCreated;
    }

    public virtual Job Job { get; } = null!;
    public virtual string EntityName { get; } = null!;
    public virtual long EntityId { get; }
    public virtual bool IsCreated { get; }

    public virtual JobAffectedEntityDto GetDto()
    {
        return new JobAffectedEntityDto(EntityName, EntityId, IsCreated);
    }
}