namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetPublicWatchdogStatisticsQueryResult
{
    public bool ReceiveNotification { get; set; }
    public long WatchdogSearchUserId { get; set; }
    public int CountOfWatchdogSearches { get; set; }
}