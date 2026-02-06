using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class WatchdogScrapedResultHistory : VersionedEntity
{
    protected WatchdogScrapedResultHistory() {}
    
    public WatchdogScrapedResultHistory(string result, DateTime notifiedOn)
    {
        Result = result;
        NotifiedOn = notifiedOn;
    }

    public virtual string Result { get; } = null!;
    public virtual DateTime NotifiedOn { get; }
}