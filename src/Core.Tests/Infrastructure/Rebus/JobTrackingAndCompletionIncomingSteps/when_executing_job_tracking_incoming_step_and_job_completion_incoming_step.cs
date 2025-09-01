using Castle.Windsor;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingAndCompletionIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_and_job_completion_incoming_step : BaseDatabaseTest
{
    private CreateWatchdogCommand _command = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageIdOne;
    private long _watchdogWebPageIdTwo;
    private IWindsorContainer _windsorContainer = null!;
    private IWindsorContainer? _jobContextWindsorContainerInTheNextIncomingStep;
    private HashSet<IDomainEvent>? _jobContextRaisedDomainEventsInTheNextIncomingStep;
    private Guid _jobContextCommandGuidInTheNextIncomingStep;
    private long _jobContentActingUserId;

    [SetUp]
    public async Task Context()
    {
        _windsorContainer = A.Fake<IWindsorContainer>();
        JobContext.RaisedDomainEvents.Value = [new TestDomainEvent()];

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

        var jobTrackingIncomingStep = new JobTrackingIncomingStepBuilder()
            .WithWindsorContainer(_windsorContainer)
            .Build();

        var jobCompletionIncomingStep = new JobCompletionIncomingStepBuilder(UnitOfWork).Build();
        
        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _command = new CreateWatchdogCommand("watchdog name")
        {
            Guid = Guid.NewGuid(),
            ActingUserId = 23
        };
        incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _command.Guid.ToString()}}, _command));

        await jobTrackingIncomingStep.Process(incomingStepContext, async () =>
        {
            _jobContentActingUserId = JobContext.ActingUserId.Value;
            
            await jobCompletionIncomingStep.Process(incomingStepContext, _next);
        });

        
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

            _jobContextWindsorContainerInTheNextIncomingStep = JobContext.WindsorContainer.Value;
            _jobContextRaisedDomainEventsInTheNextIncomingStep = JobContext.RaisedDomainEvents.Value;
            _jobContextCommandGuidInTheNextIncomingStep = JobContext.CommandGuid.Value;
        }
    }

    [Test]
    public async Task job_is_created_and_completed()
    {
        var job = await new JobRepository(UnitOfWork).GetByGuidAsync(_command.Guid);

        job.ShouldNotBeNull();
        job.CreatedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.CompletedOn.ShouldNotBeNull();
        job.CompletedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.Type.ShouldBe(nameof(CreateWatchdogCommand));
        job.InputData.ShouldBe($$"""
                               {"guid": "{{job.Guid}}", "name": "watchdog name", "actingUserId": 23}
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

    [Test]
    public void job_context_windsor_container_is_set_in_the_next_incoming_step()
    {
        _jobContextWindsorContainerInTheNextIncomingStep.ShouldBe(_windsorContainer);
    }

    [Test]
    public void job_context_raised_domain_events_are_reset_in_the_next_incoming_step()
    {
        _jobContextRaisedDomainEventsInTheNextIncomingStep.ShouldBeEmpty();
    }
    
    [Test]
    public void job_context_command_guid_is_set_in_the_next_incoming_step()
    {
        _jobContextCommandGuidInTheNextIncomingStep.ShouldBe(_command.Guid);
    }

    [Test]
    public void acting_user_id_is_set_on_job_context()
    {
        _jobContentActingUserId.ShouldBe(23);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await UnitOfWork.FlushAsync();
        await UnitOfWork.RollbackAsync();
        UnitOfWork.BeginTransaction();
        
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        var jobRepository = new JobRepository(newUnitOfWork);
        var job = await jobRepository.GetByGuidAsync(_command.Guid);
        if (job != null)
        {
            await jobRepository.DeleteAsync(job);
        }
    }

    private record TestDomainEvent : DomainEvent;
}