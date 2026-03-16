namespace MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;

public class JobContextRebusQueueRedirector : IRebusQueueRedirector
{
    public string? GetQueueForRedirection()
    {
        var rebusHandlingQueue = JobContext.RebusHandlingQueue.Value;

        if (rebusHandlingQueue == null) return null;
        
        return rebusHandlingQueue.EndsWith(RebusConstants.RebusSendQueueSuffix)
            ? $"{rebusHandlingQueue}"
            : $"{rebusHandlingQueue}{RebusConstants.RebusSendQueueSuffix}";
    }
}