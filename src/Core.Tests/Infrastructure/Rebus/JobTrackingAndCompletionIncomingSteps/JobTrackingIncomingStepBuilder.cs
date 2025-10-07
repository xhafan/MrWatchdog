using Castle.Windsor;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

public class JobTrackingIncomingStepBuilder
{
    private IWindsorContainer? _windsorContainer;
    private IRebusHandlingQueueGetter? _rebusHandlingQueueGetter;

    public JobTrackingIncomingStepBuilder WithWindsorContainer(IWindsorContainer windsorContainer)
    {
        _windsorContainer = windsorContainer;
        return this;
    }

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
            _windsorContainer ?? A.Fake<IWindsorContainer>(),
            new NewTransactionJobCreator(TestFixtureContext.NhibernateConfigurator),
            _rebusHandlingQueueGetter
        );
    }
}