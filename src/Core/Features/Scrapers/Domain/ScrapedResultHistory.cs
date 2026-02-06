using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScrapedResultHistory : VersionedEntity // todo: rename file once committed
{
    protected ScrapedResultHistory() {}
    
    public ScrapedResultHistory(string result, DateTime notifiedOn)
    {
        Result = result;
        NotifiedOn = notifiedOn;
    }

    public virtual string Result { get; } = null!;
    public virtual DateTime NotifiedOn { get; }
}