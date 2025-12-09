using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Jobs;

[TestFixture]
public class when_getting_related_domain_event_job : BaseDatabaseTest
{
    private Job _commandJob = null!;
    private Job _domainEventJob = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _commandJob = new JobBuilder(UnitOfWork)
            .WithKind(JobKind.Command)
            .Build();
        _commandJob.HandlingStarted(RebusQueues.Main);
        _commandJob.Complete();
        _commandJob.AddAffectedEntity(nameof(Scraper), 23, isCreated: true);
        
        _domainEventJob = new JobBuilder(UnitOfWork)
            .WithKind(JobKind.DomainEvent)
            .WithType(nameof(ScraperWebPageScrapingDataUpdatedDomainEvent))
            .Build();
        _domainEventJob.HandlingStarted(RebusQueues.Main);
        _domainEventJob.Complete();
        _domainEventJob.SetRelatedCommandJob(_commandJob);
        
        var controller = new JobsControllerBuilder(UnitOfWork).Build();

        _actionResult = await controller.GetRelatedDomainEventJob(_commandJob.Guid, nameof(ScraperWebPageScrapingDataUpdatedDomainEvent));
    }

    [Test]
    public void job_dto_data_are_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        
        okObjectResult.Value.ShouldBeOfType<JobDto>();
        var jobDto = (JobDto) okObjectResult.Value;
        
        jobDto.Guid.ShouldBe(_domainEventJob.Guid);
        jobDto.CreatedOn.ShouldBe(_domainEventJob.CreatedOn);
        jobDto.CompletedOn.ShouldBe(_domainEventJob.CompletedOn);
        jobDto.Type.ShouldBe(_domainEventJob.Type);
        jobDto.InputData.ShouldBe(_domainEventJob.InputData);
        jobDto.Kind.ShouldBe(_domainEventJob.Kind);
    }
}