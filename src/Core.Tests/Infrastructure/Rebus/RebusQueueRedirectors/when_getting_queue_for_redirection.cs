using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.RebusQueueRedirectors;

[TestFixture]
public class when_getting_queue_for_redirection
{
    private string? _queueForRedirection;

    [SetUp]
    public void Context()
    {
        JobContext.RebusHandlingQueue.Value = $"Test{RebusQueues.AdminBulk}";
        var httpContextRebusQueueRedirector = new JobContextRebusQueueRedirector();

        _queueForRedirection = httpContextRebusQueueRedirector.GetQueueForRedirection();
    }

    [Test]
    public void queue_for_redirection_is_correct()
    {
        _queueForRedirection.ShouldBe($"Test{RebusQueues.AdminBulk}{RebusConstants.RebusSendQueueSuffix}");
    }
}