namespace MrWatchdog.Web.Infrastructure.RateLimiting;

public class RateLimitingOptions
{
    public int GlobalRequestsPerSecondPerUserPermitLimit { get; set; }
    public int GlobalRequestsPerSecondPerUserQueueLimit { get; set; }

    public int LogErrorRequestsPerSecondPerUserPermitLimit { get; set; }
    public int LogErrorRequestsPerSecondPerUserQueueLimit { get; set; }
}