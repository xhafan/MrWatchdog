using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.RebusQueueRedirectors;

[TestFixture]
public class when_getting_queue_for_redirection_without_job_context_rebus_handling_queue_set
{
    private string? _queueForRedirection;

    [SetUp]
    public void Context()
    {
        JobContext.RebusHandlingQueue.Value = null;
        var httpContextRebusQueueRedirector = new JobContextRebusQueueRedirector();

        _queueForRedirection = httpContextRebusQueueRedirector.GetQueueForRedirection();
    }

    [Test]
    public void queue_for_redirection_is_not_set()
    {
        _queueForRedirection.ShouldBe(null);
    }
}