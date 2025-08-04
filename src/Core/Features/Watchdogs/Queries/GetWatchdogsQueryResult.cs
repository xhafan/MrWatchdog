namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogsQueryResult
{
    public required long WatchdogId { get; init; }
    public required string WatchdogName { get; init; } = null!;
}