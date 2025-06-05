using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Rebus;

[TestFixture]
public class when_executing_job_tracking_incoming_step : BaseDatabaseTest
{
    private CreateWatchdogCommand _command = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageIdOne;
    private long _watchdogWebPageIdTwo;

    [SetUp]
    public async Task Context()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageIdOne = _watchdog.WebPages.Single().Id;
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var step = new JobTrackingIncomingStep(
            TestFixtureContext.NhibernateConfigurator,
            A.Fake<ILogger<JobTrackingIncomingStep>>()
        );

        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _command = new CreateWatchdogCommand("watchdog name");
        incomingStepContext.Save(new Message(new Dictionary<string, string>(), _command));

        await step.Process(incomingStepContext, _next);

        await UnitOfWork.FlushAsync();
        return;

        async Task _next()
        {
            _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
            _watchdog.AddWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page2",
                Selector = ".selector2",
                Name = "url.com/page2"
            });
            await UnitOfWork.FlushAsync();
            
            _watchdogWebPageIdTwo = _watchdog.WebPages.Single(x => x.Id != _watchdogWebPageIdOne).Id;
        }
    }

    [Test]
    public async Task job_is_created_and_completed()
    {
        var job = await _GetJob(UnitOfWork);

        job.ShouldNotBeNull();
        job.CreatedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.CompletedOn.ShouldNotBeNull();
        job.CompletedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.Type.ShouldBe(nameof(CreateWatchdogCommand));
        job.InputData.ShouldBe($$"""
                               {"guid": "{{job.Guid}}", "name": "watchdog name"}
                               """);
        job.Kind.ShouldBe(JobKind.Command);
        job.NumberOfHandlingAttempts.ShouldBe(1);
        
        var jobAffectedEntity = job.AffectedEntities.SingleOrDefault(x => x.EntityName == nameof(Watchdog));
        jobAffectedEntity.ShouldNotBeNull();
        jobAffectedEntity.EntityId.ShouldBe(_watchdog.Id);
        
        jobAffectedEntity = job.AffectedEntities.SingleOrDefault(x => x.EntityName == nameof(WatchdogWebPage) && !x.IsCreated);
        jobAffectedEntity.ShouldBeNull();

        jobAffectedEntity = job.AffectedEntities.SingleOrDefault(x => x.EntityName == nameof(WatchdogWebPage) && x.IsCreated);
        jobAffectedEntity.ShouldNotBeNull();
        jobAffectedEntity.EntityId.ShouldBe(_watchdogWebPageIdTwo);

        var jobHandlingAttempt = job.HandlingAttempts.ShouldHaveSingleItem();
        jobHandlingAttempt.StartedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        jobHandlingAttempt.EndedOn.ShouldNotBeNull();
        jobHandlingAttempt.EndedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
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