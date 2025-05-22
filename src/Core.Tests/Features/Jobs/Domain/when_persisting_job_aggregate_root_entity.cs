using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Jobs.Domain;

[TestFixture]
public class when_persisting_job_aggregate_root_entity : BaseDatabaseTest
{
    private readonly Guid _jobGuid = Guid.NewGuid();

    private Job _newJob = null!;
    private JobAggregateRootEntity _persistedJobAggregateRootEntity = null!;

    [SetUp]
    public void Context()
    {
        _newJob = new JobBuilder(UnitOfWork)
            .WithGuid(_jobGuid)
            .Build();
        _newJob.HandlingStarted();
        _newJob.AddAffectedAggregateRootEntity(nameof(Watchdog), 23);
        UnitOfWork.Save(_newJob);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedJobAggregateRootEntity = UnitOfWork.LoadById<Job>(_newJob.Id).AffectedAggregateRootEntities.Single();
    }

    [Test]
    public void persisted_job_handling_attempt_can_be_retrieved_and_has_correct_data()
    {
        _persistedJobAggregateRootEntity.Job.ShouldBe(_newJob);
        _persistedJobAggregateRootEntity.AggregateRootEntityName.ShouldBe(nameof(Watchdog));
        _persistedJobAggregateRootEntity.AggregateRootEntityId.ShouldBe(23);
    }
}