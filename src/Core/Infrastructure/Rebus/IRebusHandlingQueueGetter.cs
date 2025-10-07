namespace MrWatchdog.Core.Infrastructure.Rebus;

public interface IRebusHandlingQueueGetter
{
    string GetHandlingQueue();
}