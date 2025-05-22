using MrWatchdog.TestsShared.Extensions;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Rebus;

[TestFixture]
public class when_executing_job_tracking_incoming_step_with_existing_job : BaseDatabaseTest
{
    private CreateWatchdogCommand _command = null!;
    private Watchdog _newWatchdog = null!;
    private Job _job = null!;

    [SetUp]
    public async Task Context()
    {
        var step = new JobTrackingIncomingStep(
            TestFixtureContext.NhibernateConfigurator,
            A.Fake<ILogger<JobTrackingIncomingStep>>()
        );

        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _command = new CreateWatchdogCommand("watchdog name");
        _CreateJobInSeparateTransaction();
        incomingStepContext.Save(new Message(new Dictionary<string, string>(), _command));

        await step.Process(incomingStepContext, _next);

        await UnitOfWork.FlushAsync();
        return;

        Task _next()
        {
            _newWatchdog = new WatchdogBuilder(UnitOfWork).Build();
            return Task.CompletedTask;
        }
    }

    [Test]
    public void existing_job_is_fetched_and_completed()
    {
        _job = _GetJob(UnitOfWork);

        _job.CompletedOn.ShouldNotBeNull();
        _job.CompletedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _job.NumberOfHandlingAttempts.ShouldBe(1);
        
        var jobAffectedAggregateRootEntity = _job.AffectedAggregateRootEntities.ShouldHaveSingleItem();
        jobAffectedAggregateRootEntity.AggregateRootEntityName.ShouldBe(nameof(Watchdog));
        jobAffectedAggregateRootEntity.AggregateRootEntityId.ShouldBe(_newWatchdog.Id);
        
        var jobHandlingAttempt = _job.HandlingAttempts.ShouldHaveSingleItem();
        jobHandlingAttempt.StartedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        jobHandlingAttempt.EndedOn.ShouldNotBeNull();
        jobHandlingAttempt.EndedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
    }

    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        var job = _GetJob(newUnitOfWork);
        await newUnitOfWork.Session!.DeleteAsync(job);
    }

    private void _CreateJobInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _job = new JobBuilder(newUnitOfWork)
            .WithGuid(_command.Guid)
            .WithType(nameof(CreateWatchdogCommand))
            .WithInputData(_command)
            .WithKind(JobKind.Command)
            .Build();
    }

    private Job _GetJob(NhibernateUnitOfWork unitOfWork)
    {
        return unitOfWork.LoadById<Job>(_job.Id);
    }
}