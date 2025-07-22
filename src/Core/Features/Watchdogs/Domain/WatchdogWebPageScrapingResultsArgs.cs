namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageScrapingResultsArgs
{
    public required string Url { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<string> SelectedElements { get; set; }
}