namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record ScrapingResultHistory
{
    protected ScrapingResultHistory() {}
    
    public ScrapingResultHistory(string result, DateTime notifiedOn)
    {
        Result = result;
        NotifiedOn = notifiedOn;
    }

    public string Result { get; } = null!;
    public DateTime NotifiedOn { get; }
}