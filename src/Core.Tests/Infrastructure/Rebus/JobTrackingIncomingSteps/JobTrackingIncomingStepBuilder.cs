using Castle.Windsor;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingIncomingSteps;

public class JobTrackingIncomingStepBuilder
{
    private IWindsorContainer? _windsorContainer;

    public JobTrackingIncomingStepBuilder WithWindsorContainer(IWindsorContainer windsorContainer)
    {
        _windsorContainer = windsorContainer;
        return this;
    }

    public JobTrackingIncomingStep Build()
    {
        return new JobTrackingIncomingStep(
            TestFixtureContext.NhibernateConfigurator,
            A.Fake<ILogger<JobTrackingIncomingStep>>(),
            _windsorContainer ?? A.Fake<IWindsorContainer>(),
            new NewTransactionJobCreator(TestFixtureContext.NhibernateConfigurator)
        );
    }
}