using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Jobs.Services.JobCompletionAwaiters;

[TestFixture]
public class when_waiting_for_job_completion : BaseDatabaseTest
{
    private Job? _job;
    private Guid _jobGuid;
    private Task _createJobTask = null!;

    [SetUp]
    public async Task Context()
    {
        _jobGuid = Guid.NewGuid();

        _createJobTask = Task.Run(async () =>
        {
            // simulate command handler in a separate transaction
            using (var unitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator))
            {
                unitOfWork.BeginTransaction();

                _job = new JobBuilder(unitOfWork)
                    .WithGuid(_jobGuid)
                    .Build();
                unitOfWork.Save(_job);

                await Task.Delay(200);
            }
            using (var unitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator))
            {
                unitOfWork.BeginTransaction();
                var jobRepository = new JobRepository(unitOfWork);
                _job = await jobRepository.LoadByIdAsync(_job.Id);

                _job.HandlingStarted();
                _job.Complete();
                await Task.Delay(200);
            }
        });
        
        var jobCompletionAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator);
        
        await jobCompletionAwaiter.WaitForJobCompletion(_jobGuid);
    }

    [Test]
    public void after_the_wait_the_job_is_completed()
    {
        UnitOfWork.Rollback();
        UnitOfWork.BeginTransaction();
        
        _job.ShouldNotBeNull();
        _job = UnitOfWork.LoadById<Job>(_job.Id);
        _job.CompletedOn.ShouldNotBe(null);
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