namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogSearchesQueryResult
{
    public required long WatchdogSearchId { get; init; }
    public required string WatchdogName { get; init; }
    public required string? SearchTerm { get; init; }
    public required bool ReceiveNotification { get; init; }
}