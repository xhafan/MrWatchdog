using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Web.Tests.Features.Jobs;

[TestFixture]
public class when_getting_job : BaseDatabaseTest
{
    private Job _job = null!;
    private IActionResult _actionResult = null!;

    [SetUp]
    public async Task Context()
    {
        _job = new JobBuilder(UnitOfWork).Build();
        _job.HandlingStarted();
        _job.Complete();
        _job.AddAffectedEntity(nameof(Watchdog), 23, isCreated: true);
        
        var controller = new JobsControllerBuilder(UnitOfWork)
            .Build();

        _actionResult = await controller.GetJob(_job.Guid);
    }

    [Test]
    public void job_dto_data_are_correct()
    {
        _actionResult.ShouldBeOfType<OkObjectResult>();
        var okObjectResult = (OkObjectResult) _actionResult;
        
        okObjectResult.Value.ShouldBeOfType<JobDto>();
        var jobDto = (JobDto) okObjectResult.Value;
        
        jobDto.Guid.ShouldBe(_job.Guid);
        jobDto.CreatedOn.ShouldBe(_job.CreatedOn);
        jobDto.CompletedOn.ShouldBe(_job.CompletedOn);
        jobDto.Type.ShouldBe(_job.Type);
        jobDto.InputData.ShouldBe(_job.InputData);
        jobDto.Kind.ShouldBe(_job.Kind);
        jobDto.NumberOfHandlingAttempts.ShouldBe(_job.NumberOfHandlingAttempts);
        
        var jobAffectedEntityDto = jobDto.AffectedEntities.ShouldHaveSingleItem();
        jobAffectedEntityDto.EntityName.ShouldBe(nameof(Watchdog));
        jobAffectedEntityDto.EntityId.ShouldBe(23);
        jobAffectedEntityDto.IsCreated.ShouldBe(true);
        
        var jobHandlingAttemptDto = jobDto.HandlingAttempts.ShouldHaveSingleItem();
        jobHandlingAttemptDto.StartedOn.ShouldBe(_job.HandlingAttempts.Single().StartedOn);
        jobHandlingAttemptDto.EndedOn.ShouldBe(_job.HandlingAttempts.Single().EndedOn);
        jobHandlingAttemptDto.Exception.ShouldBe(null);
    }
}