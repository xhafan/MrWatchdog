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
            using (var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator))
            {
                newUnitOfWork.BeginTransaction();

                _job = new JobBuilder(newUnitOfWork)
                    .WithGuid(_jobGuid)
                    .Build();
                newUnitOfWork.Save(_job);

                await Task.Delay(200);
            }
            using (var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator))
            {
                newUnitOfWork.BeginTransaction();
                var jobRepository = new JobRepository(newUnitOfWork);
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
        _job.ShouldNotBeNull();
        _job = UnitOfWork.LoadById<Job>(_job.Id);
        _job.CompletedOn.ShouldNotBe(null);
    }

    [TearDown]
    public async Task TearDown()
    {
        await _createJobTask;
        
        using var newUnitOfWork = new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator);
        newUnitOfWork.BeginTransaction();
        await newUnitOfWork.DeleteJobCascade(_jobGuid);
    }
}