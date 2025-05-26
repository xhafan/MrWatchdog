using System.Text.Json;
using CoreDdd.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class Job : VersionedEntity, IAggregateRoot
{
    private readonly ISet<JobAggregateRootEntity> _affectedAggregateRootEntities = new HashSet<JobAggregateRootEntity>();
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
        InputData = JsonSerializer.Serialize(inputData);
        Kind = kind;
    }

    public virtual Guid Guid { get; }
    public virtual DateTime CreatedOn { get; }
    public virtual DateTime? CompletedOn { get; protected set; }
    public virtual string Type { get; } = null!;
    public virtual string InputData { get; } = null!;
    public virtual JobKind Kind { get; }
    public virtual int NumberOfHandlingAttempts { get; protected set; }
    public virtual IEnumerable<JobAggregateRootEntity> AffectedAggregateRootEntities => _affectedAggregateRootEntities;
    public virtual IEnumerable<JobHandlingAttempt> HandlingAttempts => _handlingAttempts;

    public virtual void Complete()
    {
        CompletedOn = DateTime.UtcNow;
        NumberOfHandlingAttempts++;

        var lastHandlingAttempt = _GetLastJobHandlingUnfinishedAttempt();
        lastHandlingAttempt.Complete();
    }

    public virtual void AddAffectedAggregateRootEntity(
        string aggregateRootEntityName, 
        long aggregateRootEntityId
    )
    {
        if (_affectedAggregateRootEntities.Any(x => x.AggregateRootEntityName == aggregateRootEntityName
                                                    && x.AggregateRootEntityId == aggregateRootEntityId))
        {
            return;
        }

        _affectedAggregateRootEntities.Add(new JobAggregateRootEntity(this, aggregateRootEntityName, aggregateRootEntityId));
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
            _affectedAggregateRootEntities.Select(x => x.GetDto()),
            _handlingAttempts.Select(x => x.GetDto())
        );
    }
}