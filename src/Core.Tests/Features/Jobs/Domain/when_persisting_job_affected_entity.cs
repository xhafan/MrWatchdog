using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Jobs.Domain;

[TestFixture]
public class when_persisting_job_affected_entity : BaseDatabaseTest
{
    private readonly Guid _jobGuid = Guid.NewGuid();

    private Job _newJob = null!;
    private JobAffectedEntity _persistedJobAffectedEntity = null!;

    [SetUp]
    public void Context()
    {
        _newJob = new JobBuilder(UnitOfWork)
            .WithGuid(_jobGuid)
            .Build();
        _newJob.HandlingStarted();
        _newJob.AddAffectedEntity(nameof(Watchdog), 23, isCreated: true);
        UnitOfWork.Save(_newJob);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedJobAffectedEntity = UnitOfWork.LoadById<Job>(_newJob.Id).AffectedEntities.Single();
    }

    [Test]
    public void persisted_job_handling_attempt_can_be_retrieved_and_has_correct_data()
    {
        _persistedJobAffectedEntity.Job.ShouldBe(_newJob);
        _persistedJobAffectedEntity.EntityName.ShouldBe(nameof(Watchdog));
        _persistedJobAffectedEntity.EntityId.ShouldBe(23);
        _persistedJobAffectedEntity.IsCreated.ShouldBe(true);
    }
}