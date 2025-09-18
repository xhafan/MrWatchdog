namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetPublicWatchdogsQueryResult
{
    public required long WatchdogId { get; init; }
    public required string WatchdogName { get; init; } = null!;
    public required long UserId { get; init; }
}