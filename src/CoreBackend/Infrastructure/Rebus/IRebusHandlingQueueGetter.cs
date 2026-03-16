namespace CoreBackend.Infrastructure.Rebus;

public interface IRebusHandlingQueueGetter
{
    string GetHandlingQueue();
}