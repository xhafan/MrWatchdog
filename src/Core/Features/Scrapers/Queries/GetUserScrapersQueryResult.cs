using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetUserScrapersQueryResult
{
    public required long ScraperId { get; init; }
    public required string ScraperName { get; init; } = null!;
    public required PublicStatus PublicStatus { get; init; }
}