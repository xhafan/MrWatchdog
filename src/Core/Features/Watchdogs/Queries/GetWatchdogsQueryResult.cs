namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetWatchdogSearchesQueryResult
{
    public required long WatchdogSearchId { get; init; }
    public required string ScraperName { get; init; }
    public required string? SearchTerm { get; init; }
    public required bool ReceiveNotification { get; init; }
}