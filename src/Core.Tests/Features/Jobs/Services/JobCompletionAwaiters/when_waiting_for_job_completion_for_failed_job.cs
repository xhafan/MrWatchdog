using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Jobs.Services.JobCompletionAwaiters;

[TestFixture]
public class when_waiting_for_job_completion_for_failed_job : BaseDatabaseTest
{
    private Job? _job;
    private Guid _jobGuid;
    private Task _createJobTask = null!;
    private JobCompletionAwaiter _jobCompletionAwaiter = null!;

    [SetUp]
    public void Context()
    {
        _jobGuid = Guid.NewGuid();

        _createJobTask = Task.Run(async () =>
        {
            // simulate command handler in a separate transaction
            using var unitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
            unitOfWork.BeginTransaction();

            _job = new JobBuilder(unitOfWork)
                .WithGuid(_jobGuid)
                .Build();

            for (var i = 1; i <= RebusConstants.MaxDeliveryAttempts; i++)
            {
                _job.HandlingStarted();
                _job.Fail(new Exception($"Error {i}"));
            }
            await Task.Delay(200);
        });
        
        _jobCompletionAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator);
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _jobCompletionAwaiter.WaitForJobCompletion(_jobGuid));
        
        ex.Message.ShouldBe($"Job {_jobGuid} failed: System.Exception: Error 5");
    }

    [TearDown]
    public async Task TearDown()
    {
        await _createJobTask;
        
        var jobRepository = new JobRepository(UnitOfWork);
        var job = await jobRepository.GetByGuidAsync(_jobGuid);
        if (job != null)
        {
            await jobRepository.DeleteAsync(job);
        }
        
        await UnitOfWork.CommitAsync();
        UnitOfWork.BeginTransaction();
    }
}