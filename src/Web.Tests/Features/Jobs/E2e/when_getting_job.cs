using System.Net.Http.Json;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Jobs;

namespace MrWatchdog.Web.Tests.Features.Jobs.E2e;

[TestFixture]
public class when_getting_job : BaseDatabaseTest
{
    private readonly Guid _jobGuid = Guid.NewGuid();
    private Job? _job;

    [SetUp]
    public void Context()
    {
        _BuildEntitiesInSeparateTransaction();
    }

    [Test]
    public async Task job_data_is_correct()
    {
        var getJobUrl = JobUrlConstants.GetJobUrl.Replace(JobUrlConstants.JobGuidVariable, _jobGuid.ToString());
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

        await newUnitOfWork.DeleteJobCascade(_job);
    } 

    private void _BuildEntitiesInSeparateTransaction()
    {
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();

        _job = new JobBuilder(newUnitOfWork)
            .WithGuid(_jobGuid)
            .Build();
    }
}