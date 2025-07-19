namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageSelectedElementsDto(
    long WatchdogId,
    long WatchdogWebPageId,
    IEnumerable<string> SelectedElements,
    DateTime? ScrapedOn,
    string? ScrapingErrorMessage
);