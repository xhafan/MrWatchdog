using Castle.Windsor;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_with_failing_job : BaseDatabaseTest
{
    private CreateWatchdogCommand _command = null!;

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
        _command = new CreateWatchdogCommand("watchdog name") {Guid = Guid.NewGuid()};
        incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _command.Guid.ToString()}}, _command));

        try
        {
            await step.Process(incomingStepContext, _next);
        }
        catch (Exception ex)
        {
            ex.Message.ShouldBe("Test exception");
        }

        await UnitOfWork.FlushAsync();
        return;

        Task _next()
        {
            _ = new WatchdogBuilder(UnitOfWork).Build();
            throw new Exception("Test exception");
        }
    }

    [Test]
    public async Task job_is_created_and_failed()
    {
        var job = await _GetJob(UnitOfWork);

        job.ShouldNotBeNull();
        job.CreatedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.CompletedOn.ShouldBe(null);
        job.NumberOfHandlingAttempts.ShouldBe(1);
        
        job.AffectedEntities.ShouldBeEmpty();
        
        var jobHandlingAttempt = job.HandlingAttempts.ShouldHaveSingleItem();
        jobHandlingAttempt.StartedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        jobHandlingAttempt.EndedOn.ShouldNotBeNull();
        jobHandlingAttempt.EndedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        jobHandlingAttempt.Exception.ShouldStartWith("System.Exception: Test exception");
    }

    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        var job = await _GetJob(newUnitOfWork);
        if (job != null)
        {
            await newUnitOfWork.Session!.DeleteAsync(job);
        }
    }

    private async Task<Job?> _GetJob(NhibernateUnitOfWork unitOfWork)
    {
        return await unitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Guid == _command.Guid)
            .SingleOrDefaultAsync();
    }
}