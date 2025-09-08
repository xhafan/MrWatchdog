using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Jobs.Domain;

[TestFixture]
public class when_persisting_job : BaseDatabaseTest
{
    private readonly Guid _jobGuid = Guid.NewGuid();

    private Job _newJob = null!;
    private Job? _persistedJob;
    private Job _relatedCommandJob = null!;

    [SetUp]
    public void Context()
    {
        _relatedCommandJob = new JobBuilder(UnitOfWork).Build();
        
        _newJob = new JobBuilder(UnitOfWork)
            .WithGuid(_jobGuid)
            .Build();
        _newJob.HandlingStarted();
        _newJob.Complete();
        _newJob.SetRelatedCommandJob(_relatedCommandJob);
        UnitOfWork.Save(_newJob);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedJob = UnitOfWork.Get<Job>(_newJob.Id);
    }

    [Test]
    public void persisted_job_can_be_retrieved_and_has_correct_data()
    {
        _persistedJob.ShouldNotBeNull();
        _persistedJob.ShouldBe(_newJob);

        _persistedJob.Guid.ShouldBe(_jobGuid);
        _persistedJob.CreatedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _persistedJob.CompletedOn.ShouldNotBeNull();
        _persistedJob.CompletedOn.Value.ShouldBeGreaterThanOrEqualTo(_persistedJob.CreatedOn);
        _persistedJob.CompletedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _persistedJob.Type.ShouldBe(JobBuilder.Type);
        JsonHelper.Deserialize<CreateWatchdogCommand>(_persistedJob.InputData).ShouldBe(JobBuilder.InputData);
        _persistedJob.Kind.ShouldBe(JobBuilder.Kind);
        _persistedJob.NumberOfHandlingAttempts.ShouldBe(1);
        _persistedJob.AffectedEntities.ShouldBeEmpty();
        _persistedJob.HandlingAttempts.Count().ShouldBe(1);
        _persistedJob.RelatedCommandJob.ShouldBe(_relatedCommandJob);
        _persistedJob.RequestId.ShouldBe(JobBuilder.RequestId);
    }
}