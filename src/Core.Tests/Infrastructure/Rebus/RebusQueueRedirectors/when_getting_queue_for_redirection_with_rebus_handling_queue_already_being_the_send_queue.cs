using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Rebus.RebusQueueRedirectors;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.RebusQueueRedirectors;

[TestFixture]
public class when_getting_queue_for_redirection_with_rebus_handling_queue_already_being_the_send_queue
{
    private string? _queueForRedirection;

    [SetUp]
    public void Context()
    {
        JobContext.RebusHandlingQueue.Value = $"Test{RebusQueues.AdminBulk}{RebusConstants.RebusSendQueueSuffix}";
        var httpContextRebusQueueRedirector = new JobContextRebusQueueRedirector();

        _queueForRedirection = httpContextRebusQueueRedirector.GetQueueForRedirection();
    }

    [Test]
    public void queue_for_redirection_is_correct()
    {
        _queueForRedirection.ShouldBe($"Test{RebusQueues.AdminBulk}{RebusConstants.RebusSendQueueSuffix}");
    }
}