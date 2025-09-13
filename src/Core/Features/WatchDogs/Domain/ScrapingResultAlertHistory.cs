namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record ScrapingResultAlertHistory
{
    protected ScrapingResultAlertHistory() {}
    
    public ScrapingResultAlertHistory(string result, DateTime alertedOn)
    {
        Result = result;
        AlertedOn = alertedOn;
    }

    public string Result { get; } = null!;
    public DateTime AlertedOn { get; }
}