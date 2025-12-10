namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogsQueryResult
{
    public required long WatchdogId { get; init; }
    public required string ScraperName { get; init; }
    public required string? SearchTerm { get; init; }
    public required bool ReceiveNotification { get; init; }
}