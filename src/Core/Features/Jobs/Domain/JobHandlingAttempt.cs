using CoreUtils;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Jobs.Domain;

public class JobHandlingAttempt : VersionedEntity
{
    protected JobHandlingAttempt() {}

    public JobHandlingAttempt(Job job)
    {
        Job = job;
        StartedOn = DateTime.UtcNow;
    }

    public virtual Job Job { get; } = null!;
    public virtual DateTime StartedOn { get; }
    public virtual DateTime? EndedOn { get; protected set; }
    public virtual string? Exception { get; protected set; }

    public virtual void Complete()
    {
        Guard.Hope(EndedOn == null, "Job handling attempt has already ended.");
        EndedOn = DateTime.UtcNow;
    }

    public virtual void Fail(Exception ex)
    {
        Guard.Hope(EndedOn == null, "Job handling attempt has already ended.");
        EndedOn = DateTime.UtcNow;
        Exception = ex.ToString();
    }
}