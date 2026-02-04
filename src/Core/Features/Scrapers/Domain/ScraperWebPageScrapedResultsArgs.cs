namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageScrapingResultsArgs
{
    public required string Url { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<string> ScrapingResults { get; set; }
}