using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetUserWatchdogsQueryResult
{
    public required long WatchdogId { get; init; }
    public required string WatchdogName { get; init; } = null!;
    public required PublicStatus PublicStatus { get; init; }
}