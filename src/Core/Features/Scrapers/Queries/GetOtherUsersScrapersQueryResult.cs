using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetOtherUsersScrapersQueryResult
{
    public required long ScraperId { get; init; }
    public required string ScraperName { get; init; }
    public required PublicStatus PublicStatus { get; init; }
    public required long UserId { get; init; }
    public required string UserEmail { get; init; }
}