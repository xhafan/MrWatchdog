namespace CoreBackend.Infrastructure.Rebus.RebusQueueRedirectors;

public interface IRebusQueueRedirector
{
    string? GetQueueForRedirection();
}