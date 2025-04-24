using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Users.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    protected WatchdogAlert() {}

    public WatchdogAlert(Watchdog watchdog)
    {
    }
    
    public virtual Watchdog Watchdog { get; protected set; } = null!;
    public virtual User User { get; protected set; } = null!;
    public virtual string? SearchTerm { get; protected set; } = null!;
    public virtual int SearchIntervalInSeconds { get; protected set; }
    public virtual AlertFrequency Frequency { get; protected set; }
}