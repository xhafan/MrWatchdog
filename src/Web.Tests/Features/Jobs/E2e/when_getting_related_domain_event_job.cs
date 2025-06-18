using System.Net.Http.Json;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;

namespace MrWatchdog.Web.Tests.Features.Jobs.E2e;

[TestFixture]
public class when_getting_related_domain_event_job
{
    private readonly Guid _commandJobGuid = Guid.NewGuid();
    private Job _commandJob = null!;
    private Job _domainEventJob = null!;

    [SetUp]
    public void Context()
    {
        using var unitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        unitOfWork.BeginTransaction();

        _commandJob = new JobBuilder(unitOfWork)
            .WithGuid(_commandJobGuid)
            .Build();
        
        _domainEventJob = new JobBuilder(unitOfWork)
            .WithType(nameof(WatchdogWebPageUpdatedDomainEvent))
            .Build();
        _domainEventJob.SetRelatedCommandJob(_commandJob);
    }

    [Test]
    public async Task domain_event_job_data_is_correct()
    {
        var getRelatedDomainEventJobUrl = JobConstants.GetRelatedDomainEventJobUrl
            .Replace(JobConstants.CommandJobGuidVariable, _commandJobGuid.ToString())
            .Replace(JobConstants.DomainEventTypeVariable, nameof(WatchdogWebPageUpdatedDomainEvent));
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(getRelatedDomainEventJobUrl);
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
        var jobRepository = new JobRepository(newUnitOfWork);

        _domainEventJob = await jobRepository.LoadByGuidAsync(_domainEventJob.Guid);
        await jobRepository.DeleteAsync(_domainEventJob);

        _commandJob = await jobRepository.LoadByGuidAsync(_commandJobGuid);
        await jobRepository.DeleteAsync(_commandJob);
    }    
}