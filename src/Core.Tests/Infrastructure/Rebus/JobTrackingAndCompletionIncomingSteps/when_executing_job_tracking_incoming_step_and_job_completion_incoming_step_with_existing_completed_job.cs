using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_and_job_completion_incoming_step_with_existing_completed_job : BaseDatabaseTest
{
    private CreateWatchdogCommand _command = null!;
    private Job _job = null!;
    private JobTrackingIncomingStep _jobTrackingIncomingStep = null!;
    private JobCompletionIncomingStep _jobCompletionIncomingStep = null!;
    private IncomingStepContext _incomingStepContext = null!;
    private bool _isNextStepExecuted;

    [SetUp]
    public async Task Context()
    {
        _jobTrackingIncomingStep = new JobTrackingIncomingStepBuilder()
            .Build();
        
        _jobCompletionIncomingStep = new JobCompletionIncomingStepBuilder(UnitOfWork).Build();

        _incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _command = new CreateWatchdogCommand(UserId: 23, "watchdog name") {Guid = Guid.NewGuid()};
        _BuildEntitiesInSeparateTransaction();
        _incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _command.Guid.ToString()}}, _command));

        await _jobTrackingIncomingStep.Process(_incomingStepContext,
            async () =>
            {
                await _jobCompletionIncomingStep.Process(_incomingStepContext, _next);
            });
        
        Task _next()
        {
            _isNextStepExecuted = true;
            return Task.CompletedTask;
        } 

    }

    [Test]
    public void next_incoming_step_is_not_executed()
    {
        _isNextStepExecuted.ShouldBe(false);
    }

    [TearDown]
    public async Task TearDown()
    {
        await UnitOfWork.FlushAsync();
        await UnitOfWork.RollbackAsync();
        UnitOfWork.BeginTransaction();
        
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        await newUnitOfWork.DeleteJobCascade(_job);
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _job = new JobBuilder(newUnitOfWork)
            .WithGuid(_command.Guid)
            .WithType(nameof(CreateWatchdogCommand))
            .WithInputData(_command)
            .WithKind(JobKind.Command)
            .Build();
        _job.HandlingStarted();
        _job.Complete();
        newUnitOfWork.Save(_job);
    }
}