namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageScrapingResultsDto(
    long ScraperId,
    long ScraperWebPageId,
    IEnumerable<string> ScrapingResults,
    DateTime? ScrapedOn,
    string? ScrapingErrorMessage
);