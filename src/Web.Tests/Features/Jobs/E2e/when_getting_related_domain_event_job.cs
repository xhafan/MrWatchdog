using System.Net.Http.Json;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;

namespace MrWatchdog.Web.Tests.Features.Jobs.E2e;

[TestFixture]
public class when_getting_related_domain_event_job : BaseDatabaseTest
{
    private readonly Guid _commandJobGuid = Guid.NewGuid();
    private Job _commandJob = null!;
    private Job _domainEventJob = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task domain_event_job_data_is_correct()
    {
        var url = JobUrlConstants.GetRelatedDomainEventJobUrlTemplate
            .WithCommandJobGuid(_commandJobGuid)
            .WithDomainEventType(nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent));
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var jobDto = await response.Content.ReadFromJsonAsync<JobDto>();
        jobDto.ShouldNotBeNull();
        jobDto.Guid.ShouldBe(_domainEventJob.Guid);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        await newUnitOfWork.DeleteJobCascade(_domainEventJob.Guid);
        await newUnitOfWork.DeleteJobCascade(_commandJobGuid);
    } 

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _commandJob = new JobBuilder(newUnitOfWork)
            .WithGuid(_commandJobGuid)
            .Build();
        
        _domainEventJob = new JobBuilder(newUnitOfWork)
            .WithType(nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent))
            .Build();
        _domainEventJob.SetRelatedCommandJob(_commandJob);
    }
}