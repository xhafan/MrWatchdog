using Castle.Windsor;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_with_non_base_message : BaseDatabaseTest
{
    private bool _isNextStepExecuted;

    [SetUp]
    public async Task Context()
    {
        var step = new JobTrackingIncomingStep(
            TestFixtureContext.NhibernateConfigurator,
            A.Fake<ILogger<JobTrackingIncomingStep>>(),
            A.Fake<IWindsorContainer>()
        );

        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        incomingStepContext.Save(new Message(new Dictionary<string, string>(), new object()));

        await step.Process(incomingStepContext, _next);


        Task _next()
        {
            _isNextStepExecuted = true;
            return Task.CompletedTask;
        }
    }

    [Test]
    public void next_step_is_executed()
    {
        _isNextStepExecuted.ShouldBe(true);
    }
}