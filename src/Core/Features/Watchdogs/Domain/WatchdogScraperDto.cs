namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogScraperDto
{
    public required long WatchdogId { get; set; }

    public required bool ScrapedResultsFilteringNotSupported { get; set; }
}