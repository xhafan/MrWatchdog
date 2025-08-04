namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageScrapingResultsDto(
    long WatchdogId,
    long WatchdogWebPageId,
    IEnumerable<string> ScrapingResults,
    DateTime? ScrapedOn,
    string? ScrapingErrorMessage
);