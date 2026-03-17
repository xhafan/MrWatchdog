using CoreBackend.Features.Jobs.Domain;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Repositories;
using CoreDdd.Nhibernate.TestHelpers;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

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
            await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
                async newUnitOfWork =>
                {
                    _job = new JobBuilder(newUnitOfWork)
                        .WithGuid(_jobGuid)
                        .Build();
                    newUnitOfWork.Save(_job);

                    await Task.Delay(200);
                }
            );
            await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
                async newUnitOfWork =>
                {
                    var jobRepository = new JobRepository(newUnitOfWork);
                    _job = await jobRepository.LoadByIdAsync(_job!.Id);

                    _job.HandlingStarted(RebusQueues.Main);
                    _job.Complete();
                    await Task.Delay(200);
                }
            );
        });
        
        var rebusOptions = OptionsTestRetriever.Retrieve<RebusOptions>();
        var jobCompletionAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator, rebusOptions);
        
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

        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteJobCascade(_jobGuid);
            }
        );
    }
}