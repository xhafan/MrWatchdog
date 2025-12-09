using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetOtherUsersWatchdogsQueryResult
{
    public required long WatchdogId { get; init; }
    public required string WatchdogName { get; init; } = null!;
    public required PublicStatus PublicStatus { get; init; }
    public required long UserId { get; init; }
    public required string UserEmail { get; init; } = null!;
}