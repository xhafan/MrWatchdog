using Castle.Windsor;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using Microsoft.Extensions.Logging;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Messages;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;
using Rebus.Messages;
using Rebus.Pipeline;
using Rebus.Transport;

namespace MrWatchdog.Core.Tests.Infrastructure.Rebus.JobTrackingIncomingSteps;

[TestFixture]
public class when_executing_job_tracking_incoming_step_for_domain_event : BaseDatabaseTest
{
    private WatchdogWebPageUpdatedDomainEvent _domainEvent = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageIdOne;
    private long _watchdogWebPageIdTwo;
    private IWindsorContainer _windsorContainer = null!;
    private IWindsorContainer? _jobContextWindsorContainerInTheNextIncomingStep;
    private HashSet<IDomainEvent>? _jobContextRaisedDomainEventsInTheNextIncomingStep;
    private Guid _jobContextCommandGuidInTheNextIncomingStep;
    private readonly Guid _relatedCommandGuid = Guid.NewGuid();
    private readonly Guid _domainEventJobGuid = Guid.NewGuid();
    private Job _commandJob = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildCommandJob();
        
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

        var step = new JobTrackingIncomingStep(
            TestFixtureContext.NhibernateConfigurator,
            A.Fake<ILogger<JobTrackingIncomingStep>>(),
            _windsorContainer
        );

        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _domainEvent = new WatchdogWebPageUpdatedDomainEvent(WatchdogId: _watchdog.Id, WatchdogWebPageId: _watchdogWebPageIdOne)
        {
            RelatedCommandGuid = _relatedCommandGuid
        };
        incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _domainEventJobGuid.ToString()}}, _domainEvent));

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

            _jobContextWindsorContainerInTheNextIncomingStep = JobContext.WindsorContainer.Value;
            _jobContextRaisedDomainEventsInTheNextIncomingStep = JobContext.RaisedDomainEvents.Value;
            _jobContextCommandGuidInTheNextIncomingStep = JobContext.CommandGuid.Value;
        }
    }

    [Test]
    public async Task job_is_created_and_completed()
    {
        var job = await new JobRepository(UnitOfWork).GetByGuidAsync(_domainEventJobGuid);

        job.ShouldNotBeNull();
        job.CreatedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.CompletedOn.ShouldNotBeNull();
        job.CompletedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        job.Type.ShouldBe(nameof(WatchdogWebPageUpdatedDomainEvent));
        job.InputData.ShouldBe($$"""
                                 {"watchdogId": {{_watchdog.Id}}, "watchdogWebPageId": {{_watchdogWebPageIdOne}}, "relatedCommandGuid": "{{_relatedCommandGuid}}"}
                                 """);
        job.Kind.ShouldBe(JobKind.DomainEvent);
        job.NumberOfHandlingAttempts.ShouldBe(1);
        job.RelatedCommandJob.ShouldBe(_commandJob);
        
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
        _jobContextCommandGuidInTheNextIncomingStep.ShouldBe(_domainEvent.RelatedCommandGuid);
    }
 
    private void _BuildCommandJob()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        
        _commandJob = new JobBuilder(newUnitOfWork)
            .WithGuid(_relatedCommandGuid)
            .Build();        
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        var jobRepository = new JobRepository(newUnitOfWork);
        
        var domainEventJob = await jobRepository.GetByGuidAsync(_domainEventJobGuid);
        if (domainEventJob != null)
        {
            await jobRepository.DeleteAsync(domainEventJob);
        }
        
        var commandJob = await jobRepository.GetByGuidAsync(_relatedCommandGuid);
        if (commandJob != null)
        {
            await jobRepository.DeleteAsync(commandJob);
        }        
    }

    private record TestDomainEvent : DomainEvent;
}