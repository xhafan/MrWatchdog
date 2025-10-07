using CoreUtils;

namespace MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;

public class JobContextRebusQueueRedirector : IRebusQueueRedirector
{
    public string GetQueueForRedirection()
    {
        var rebusHandlingQueue = JobContext.RebusHandlingQueue.Value;
        
        Guard.Hope(rebusHandlingQueue != null, "Rebus handling queue is not set on the job context.");

        return rebusHandlingQueue.EndsWith(RebusConstants.RebusSendQueueSuffix)
            ? $"{rebusHandlingQueue}"
            : $"{rebusHandlingQueue}{RebusConstants.RebusSendQueueSuffix}";
    }
}