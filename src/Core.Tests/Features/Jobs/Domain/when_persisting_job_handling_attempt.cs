using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Jobs.Domain;

[TestFixture]
public class when_persisting_job_handling_attempt : BaseDatabaseTest
{
    private readonly Guid _jobGuid = Guid.NewGuid();

    private Job _newJob = null!;
    private JobHandlingAttempt _persistedJobHandlingAttempt = null!;

    [SetUp]
    public void Context()
    {
        _newJob = new JobBuilder(UnitOfWork)
            .WithGuid(_jobGuid)
            .Build();
        _newJob.HandlingStarted();
        _newJob.Fail(new Exception("Test exception"));
        UnitOfWork.Save(_newJob);
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedJobHandlingAttempt = UnitOfWork.LoadById<Job>(_newJob.Id).HandlingAttempts.Single();
    }

    [Test]
    public void persisted_job_handling_attempt_can_be_retrieved_and_has_correct_data()
    {
        _persistedJobHandlingAttempt.Job.ShouldBe(_newJob);
        _persistedJobHandlingAttempt.StartedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
        _persistedJobHandlingAttempt.EndedOn.ShouldNotBeNull();
        _persistedJobHandlingAttempt.EndedOn.Value.ShouldBeGreaterThanOrEqualTo(_persistedJobHandlingAttempt.StartedOn);
        _persistedJobHandlingAttempt.Exception.ShouldBe("System.Exception: Test exception");
    }
}