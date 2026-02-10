namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageScrapedResultsDto(
    long ScraperId,
    long ScraperWebPageId,
    IEnumerable<string> ScrapedResults,
    DateTime? ScrapedOn,
    string? ScrapingErrorMessage,
    bool IsEmptyWebPage
);