namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogsQueryResult
{
    public required long Id { get; init; }
    public required string Name { get; init; } = null!;
}