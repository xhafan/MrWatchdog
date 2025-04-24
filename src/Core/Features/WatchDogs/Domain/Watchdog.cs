using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class Watchdog : VersionedEntity, IAggregateRoot
{
    private readonly ISet<WatchdogWeb> _watchdogWebs = new HashSet<WatchdogWeb>();

    protected Watchdog() {}

    public virtual IEnumerable<WatchdogWeb> WatchdogWebs => _watchdogWebs;
}