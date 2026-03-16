using CoreBackend.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class WatchdogScrapedResultHistory : VersionedEntity
{
    protected WatchdogScrapedResultHistory() {}
    
    public WatchdogScrapedResultHistory(ScrapedResult scrapedResult, DateTime notifiedOn)
    {
        ScrapedResult = scrapedResult;
        NotifiedOn = notifiedOn;
    }

    public virtual ScrapedResult ScrapedResult { get; } = null!;
    public virtual DateTime NotifiedOn { get; }
}