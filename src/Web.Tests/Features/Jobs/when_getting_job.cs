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
    private JobDto _jobDto = null!;

    [SetUp]
    public async Task Context()
    {
        _job = new JobBuilder(UnitOfWork).Build();
        _job.HandlingStarted();
        _job.Complete();
        _job.AddAffectedAggregateRootEntity(nameof(Watchdog), 23);
        
        var controller = new JobsControllerBuilder(UnitOfWork)
            .Build();

        _jobDto = await controller.GetJob(_job.Guid);
    }

    [Test]
    public void job_dto_data_are_correct()
    {
        _jobDto.Guid.ShouldBe(_job.Guid);
        _jobDto.CreatedOn.ShouldBe(_job.CreatedOn);
        _jobDto.CompletedOn.ShouldBe(_job.CompletedOn);
        _jobDto.Type.ShouldBe(_job.Type);
        _jobDto.InputData.ShouldBe(_job.InputData);
        _jobDto.Kind.ShouldBe(_job.Kind);
        _jobDto.NumberOfHandlingAttempts.ShouldBe(_job.NumberOfHandlingAttempts);
        
        var jobAggregateRootEntityDto = _jobDto.AffectedAggregateRootEntities.ShouldHaveSingleItem();
        jobAggregateRootEntityDto.AggregateRootEntityName.ShouldBe(nameof(Watchdog));
        jobAggregateRootEntityDto.AggregateRootEntityId.ShouldBe(23);
        
        var jobHandlingAttemptDto = _jobDto.HandlingAttempts.ShouldHaveSingleItem();
        jobHandlingAttemptDto.StartedOn.ShouldBe(_job.HandlingAttempts.Single().StartedOn);
        jobHandlingAttemptDto.EndedOn.ShouldBe(_job.HandlingAttempts.Single().EndedOn);
        jobHandlingAttemptDto.Exception.ShouldBe(null);
    }
}