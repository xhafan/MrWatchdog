namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogAlertsQueryResult
{
    public required long WatchdogAlertId { get; init; }
    public required string WatchdogName { get; init; }
    public required string? SearchTerm { get; init; }
}