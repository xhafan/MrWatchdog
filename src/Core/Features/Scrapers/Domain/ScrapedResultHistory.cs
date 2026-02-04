namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScrapedResultHistory
{
    protected ScrapedResultHistory() {}
    
    public ScrapedResultHistory(string result, DateTime notifiedOn)
    {
        Result = result;
        NotifiedOn = notifiedOn;
    }

    public string Result { get; } = null!;
    public DateTime NotifiedOn { get; }
}