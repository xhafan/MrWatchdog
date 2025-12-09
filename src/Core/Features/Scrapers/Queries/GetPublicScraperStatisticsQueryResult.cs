namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetPublicScraperStatisticsQueryResult
{
    public bool ReceiveNotification { get; set; }
    public long WatchdogSearchUserId { get; set; }
    public int CountOfWatchdogSearches { get; set; }
}