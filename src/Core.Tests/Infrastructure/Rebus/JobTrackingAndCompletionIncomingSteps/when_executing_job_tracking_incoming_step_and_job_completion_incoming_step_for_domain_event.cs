using Castle.Windsor;
using CoreDdd.Domain.Events;
using CoreDdd.Nhibernate.UnitOfWorks;
using FakeItEasy;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
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
public class when_executing_job_tracking_incoming_step_and_job_completion_incoming_step_for_domain_event : BaseDatabaseTest
{
    private ScraperWebPageScrapingDataUpdatedDomainEvent _domainEvent = null!;
    private Scraper _scraper = null!;
    private long _scraperWebPageIdOne;
    private long _scraperWebPageIdTwo;
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
        _BuildEntitiesInSeparateTransaction();
        
        _windsorContainer = A.Fake<IWindsorContainer>();
        JobContext.RaisedDomainEvents.Value = [new TestDomainEvent()];

        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _scraperWebPageIdOne = _scraper.WebPages.Single().Id;
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        var jobTrackingIncomingStep = new JobTrackingIncomingStepBuilder()
            .WithWindsorContainer(_windsorContainer)
            .Build();
        
        var jobCompletionIncomingStep = new JobCompletionIncomingStepBuilder(UnitOfWork).Build();

        var incomingStepContext = new IncomingStepContext(
            new TransportMessage(new Dictionary<string, string>(), []), A.Fake<ITransactionContext>()
        );
        _domainEvent = new ScraperWebPageScrapingDataUpdatedDomainEvent(ScraperId: _scraper.Id, ScraperWebPageId: _scraperWebPageIdOne)
        {
            RelatedCommandGuid = _relatedCommandGuid,
            ActingUserId = 23
        };
        incomingStepContext.Save(new Message(new Dictionary<string, string> {{Headers.MessageId, _domainEventJobGuid.ToString()}}, _domainEvent));

        await jobTrackingIncomingStep.Process(incomingStepContext, async () =>
        {
            await jobCompletionIncomingStep.Process(incomingStepContext, _next);
        });

        await UnitOfWork.FlushAsync();
        return;

        async Task _next()
        {
            _scraper = UnitOfWork.LoadById<Scraper>(_scraper.Id);
            _scraper.AddWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page2",
                Selector = ".selector2",
                Name = "url.com/page2"
            });
            await UnitOfWork.FlushAsync();
            
            _scraperWebPageIdTwo = _scraper.WebPages.Single(x => x.Id != _scraperWebPageIdOne).Id;

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
        job.Type.ShouldBe(nameof(ScraperWebPageScrapingDataUpdatedDomainEvent));
        job.InputData.ShouldBe(
            $$"""
            {"requestId": null, "scraperId": {{_scraper.Id}}, "actingUserId": 23, "scraperWebPageId": {{_scraperWebPageIdOne}}, "relatedCommandGuid": "{{_relatedCommandGuid}}"}
            """);
        job.Kind.ShouldBe(JobKind.DomainEvent);
        job.NumberOfHandlingAttempts.ShouldBe(1);
        job.RelatedCommandJob.ShouldBe(_commandJob);
        
        var jobAffectedEntity = job.AffectedEntities.SingleOrDefault(x => x.EntityName == nameof(Scraper));
        jobAffectedEntity.ShouldNotBeNull();
        jobAffectedEntity.EntityId.ShouldBe(_scraper.Id);
        
        jobAffectedEntity = job.AffectedEntities.SingleOrDefault(x => x.EntityName == nameof(ScraperWebPage) && !x.IsCreated);
        jobAffectedEntity.ShouldBeNull();

        jobAffectedEntity = job.AffectedEntities.SingleOrDefault(x => x.EntityName == nameof(ScraperWebPage) && x.IsCreated);
        jobAffectedEntity.ShouldNotBeNull();
        jobAffectedEntity.EntityId.ShouldBe(_scraperWebPageIdTwo);

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
                await newUnitOfWork.DeleteJobCascade(_domainEventJobGuid);
                await newUnitOfWork.DeleteJobCascade(_relatedCommandGuid);
            }
        );
    }

    private void _BuildEntitiesInSeparateTransaction()
    {
        NhibernateUnitOfWorkRunner.Run(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            newUnitOfWork =>
            {
                _commandJob = new JobBuilder(newUnitOfWork)
                    .WithGuid(_relatedCommandGuid)
                    .Build();
            }
        );
    }
    
    private record TestDomainEvent : DomainEvent;
}