namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperWebPageScrapedResultsArgs
{
    public required string Url { get; set; }
    public required string Name { get; set; }
    public required IEnumerable<string> ScrapedResults { get; set; }
}