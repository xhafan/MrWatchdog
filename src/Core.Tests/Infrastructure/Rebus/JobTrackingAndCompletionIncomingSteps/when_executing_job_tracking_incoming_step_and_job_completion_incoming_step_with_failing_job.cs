using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_and_job_completion_incoming_step_with_failing_job : BaseDatabaseTest
{
    private CreateWatchdogCommand _command = null!;

    [SetUp]
    public async Task Context()
    {
        var jobTrackingIncomingStep = new JobTrackingIncomingStepBuilder()
            .Build();
        
        var jobCompletionIncomingStep = new JobCompletionIncomingStepBuilder(UnitOfWork).Build();
        
        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _command = new CreateWatchdogCommand(UserId: 23, "watchdog name") {Guid = Guid.NewGuid()};
        incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _command.Guid.ToString()}}, _command));

        try
        {
            await jobTrackingIncomingStep.Process(incomingStepContext, async () =>
            {
                await jobCompletionIncomingStep.Process(incomingStepContext, _next);
            });
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
        var job = await new JobRepository(UnitOfWork).GetByGuidAsync(_command.Guid);

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
        await UnitOfWork.FlushAsync();
        await UnitOfWork.RollbackAsync();
        UnitOfWork.BeginTransaction();

        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteJobCascade(_command.Guid);
            }
        );
    }
}