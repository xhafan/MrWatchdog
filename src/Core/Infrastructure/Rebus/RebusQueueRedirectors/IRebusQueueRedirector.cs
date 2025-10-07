namespace MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;

public interface IRebusQueueRedirector
{
    string? GetQueueForRedirection();
}