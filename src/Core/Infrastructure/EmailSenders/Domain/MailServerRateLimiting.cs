using CoreDdd.Domain;

namespace MrWatchdog.Core.Infrastructure.EmailSenders.Domain;

public class MailServerRateLimiting : Entity<long>, IAggregateRoot
{
    protected MailServerRateLimiting() {}

    public MailServerRateLimiting(
        string mailServerName, 
        DateTime lastRateLimitedOn
    )
    {
        MailServerName = mailServerName;
        SetLastRateLimitedOn(lastRateLimitedOn);
    }

    public virtual string MailServerName { get; } = null!;
    public virtual DateTime LastRateLimitedOn { get; protected set; }

    public virtual void SetLastRateLimitedOn(DateTime lastRateLimitedOn)
    {
        LastRateLimitedOn = lastRateLimitedOn;
    }
}