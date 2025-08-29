using System.Net.Http.Json;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Infrastructure.Repositories;
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
        _commandJob = new JobBuilder(UnitOfWork)
            .WithGuid(_commandJobGuid)
            .Build();
        
        _domainEventJob = new JobBuilder(UnitOfWork)
            .WithType(nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent))
            .Build();
        _domainEventJob.SetRelatedCommandJob(_commandJob);
        
        UnitOfWork.Commit();
        UnitOfWork.BeginTransaction();
    }

    [Test]
    public async Task domain_event_job_data_is_correct()
    {
        var getRelatedDomainEventJobUrl = JobUrlConstants.GetRelatedDomainEventJobUrl
            .Replace(JobUrlConstants.CommandJobGuidVariable, _commandJobGuid.ToString())
            .Replace(JobUrlConstants.DomainEventTypeVariable, nameof(WatchdogWebPageScrapingDataUpdatedDomainEvent));
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(getRelatedDomainEventJobUrl);
        response.EnsureSuccessStatusCode();
        var jobDto = await response.Content.ReadFromJsonAsync<JobDto>();
        jobDto.ShouldNotBeNull();
        jobDto.Guid.ShouldBe(_domainEventJob.Guid);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        var jobRepository = new JobRepository(UnitOfWork);

        var domainEventJob = await jobRepository.GetByGuidAsync(_domainEventJob.Guid);
        if (domainEventJob != null)
        {
            await jobRepository.DeleteAsync(domainEventJob); 
        }

        var commandJob = await jobRepository.GetByGuidAsync(_commandJobGuid);
        if (commandJob != null)
        {
            await jobRepository.DeleteAsync(commandJob);
        }

        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();        
    }    
}