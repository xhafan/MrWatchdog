using CoreDdd.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class Job : VersionedEntity, IAggregateRoot
{
    private readonly ISet<JobAffectedEntity> _affectedEntities = new HashSet<JobAffectedEntity>();
    private readonly ISet<JobHandlingAttempt> _handlingAttempts = new HashSet<JobHandlingAttempt>();
    
    protected Job() {}

    public Job(
        Guid guid,
        string type,
        object inputData,
        JobKind kind
    )
    {
        Guid = guid;
        CreatedOn = DateTime.UtcNow;
        Type = type;
        InputData = JsonHelper.Serialize(inputData);
        Kind = kind;
    }

    public virtual Guid Guid { get; }
    public virtual DateTime CreatedOn { get; }
    public virtual DateTime? CompletedOn { get; protected set; }
    public virtual string Type { get; } = null!;
    public virtual string InputData { get; } = null!;
    public virtual JobKind Kind { get; }
    public virtual int NumberOfHandlingAttempts { get; protected set; }
    public virtual IEnumerable<JobAffectedEntity> AffectedEntities => _affectedEntities;
    public virtual IEnumerable<JobHandlingAttempt> HandlingAttempts => _handlingAttempts;

    public virtual void Complete()
    {
        CompletedOn = DateTime.UtcNow;
        NumberOfHandlingAttempts++;

        var lastHandlingAttempt = _GetLastJobHandlingUnfinishedAttempt();
        lastHandlingAttempt.Complete();
    }

    public virtual void AddAffectedEntity(
        string entityName, 
        long entityId,
        bool isCreated
    )
    {
        if (_affectedEntities.Any(x => x.EntityName == entityName
                                       && x.EntityId == entityId))
        {
            return;
        }

        _affectedEntities.Add(new JobAffectedEntity(this, entityName, entityId, isCreated));
    }

    public virtual void HandlingStarted()
    {
        _handlingAttempts.Add(new JobHandlingAttempt(this));
    }
    
    private JobHandlingAttempt _GetLastJobHandlingUnfinishedAttempt()
    {
        var lastJobHandlingUnfinishedAttempt = _handlingAttempts.Single(x => x.EndedOn == null);
        return lastJobHandlingUnfinishedAttempt;
    }

    public virtual void Fail(Exception ex)
    {
        NumberOfHandlingAttempts++;

        var lastHandlingAttempt = _GetLastJobHandlingUnfinishedAttempt();
        lastHandlingAttempt.Fail(ex);
    }

    public virtual JobDto GetDto()
    {
        return new JobDto(
            Guid,
            CreatedOn,
            CompletedOn,
            Type,
            InputData,
            Kind,
            NumberOfHandlingAttempts,
            _affectedEntities.Select(x => x.GetDto()),
            _handlingAttempts.Select(x => x.GetDto())
        );
    }
}