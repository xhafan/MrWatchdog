namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetPublicScraperStatisticsQueryResult
{
    public bool ReceiveNotification { get; set; }
    public long WatchdogUserId { get; set; }
    public int CountOfWatchdogs { get; set; }
}