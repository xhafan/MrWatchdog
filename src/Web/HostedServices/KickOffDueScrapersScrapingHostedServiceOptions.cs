namespace MrWatchdog.Web.HostedServices;

public class KickOffDueScrapersScrapingHostedServiceOptions
{
    public bool IsDisabled { get; set; }
    public int DelayBetweenKickOffsInSeconds { get; set; }
}