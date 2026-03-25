using CoreBackend.Infrastructure.Rebus;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

public class JobTrackingIncomingStepBuilder
{
    private IRebusHandlingQueueGetter? _rebusHandlingQueueGetter;

    public JobTrackingIncomingStepBuilder WithRebusHandlingQueueGetter(IRebusHandlingQueueGetter rebusHandlingQueueGetter)
    {
        _rebusHandlingQueueGetter = rebusHandlingQueueGetter;
        return this;
    }

    public JobTrackingIncomingStep Build()
    {
        if (_rebusHandlingQueueGetter == null)
        {
            _rebusHandlingQueueGetter = A.Fake<IRebusHandlingQueueGetter>();
            A.CallTo(() => _rebusHandlingQueueGetter.GetHandlingQueue()).Returns($"Test{RebusQueues.Main}");
        }

        return new JobTrackingIncomingStep(
            TestFixtureContext.NhibernateConfigurator,
            A.Fake<ILogger<JobTrackingIncomingStep>>(),
            new NewTransactionJobCreator(TestFixtureContext.NhibernateConfigurator),
            _rebusHandlingQueueGetter
        );
    }
}