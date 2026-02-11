namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetPublicScrapersQueryResult
{
    public required long ScraperId { get; init; }
    public required string ScraperName { get; init; }
    public required long UserId { get; init; }
}