using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Jobs.Services.JobCompletionAwaiters;

[TestFixture]
public class when_waiting_for_job_completion_with_job_completing_after_timeout : BaseDatabaseTest
{
    private Job? _job;
    private Guid _jobGuid;
    private Task _createJobTask = null!;
    private JobCompletionAwaiter _jobCompletionAwaiter = null!;

    [SetUp]
    public void Context()
    {
        _jobGuid = Guid.NewGuid();

        _createJobTask = Task.Run(() =>
        {
            // simulate command handler in a separate transaction
            using var unitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
            unitOfWork.BeginTransaction();

            _job = new JobBuilder(unitOfWork)
                .WithGuid(_jobGuid)
                .Build();
        });
        
        _jobCompletionAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator);
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(async () => await _jobCompletionAwaiter.WaitForJobCompletion(_jobGuid, timeoutInMilliseconds: 10));
        
        ex.Message.ShouldBe($"Job {_jobGuid} completion timeout.");
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