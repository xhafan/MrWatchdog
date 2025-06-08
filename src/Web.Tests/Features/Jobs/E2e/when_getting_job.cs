using System.Net.Http.Json;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;

namespace MrWatchdog.Web.Tests.Features.Jobs.E2e;

[TestFixture]
public class when_getting_job
{
    private readonly Guid _jobGuid = Guid.NewGuid();
    private Job? _job;

    [SetUp]
    public void Context()
    {
        using var unitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        unitOfWork.BeginTransaction();

        _job = new JobBuilder(unitOfWork)
            .WithGuid(_jobGuid)
            .Build();
    }

    [Test]
    public async Task job_data_is_correct()
    {
        var getJobUrl = JobConstants.GetJobUrl.Replace(JobConstants.JobGuidVariable, _jobGuid.ToString());
        var response = await RunOncePerTestRun.WebApplicationClient.Value.GetAsync(getJobUrl);
        response.EnsureSuccessStatusCode();
        var jobDto = await response.Content.ReadFromJsonAsync<JobDto>();
        jobDto.ShouldNotBeNull();
        jobDto.Guid.ShouldBe(_jobGuid);
    }
    
    [TearDown]
    public async Task TearDown()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        var jobRepository = new JobRepository(newUnitOfWork);
        _job = await jobRepository.GetByGuidAsync(_jobGuid);
        if (_job != null)
        {
            await jobRepository.DeleteAsync(_job);
        }
    }    
}