namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageSelectedHtmlDto(
    long WatchdogId,
    long WatchdogWebPageId,
    string? SelectedHtml,
    DateTime? ScrapedOn,
    string? ScrapingErrorMessage
);