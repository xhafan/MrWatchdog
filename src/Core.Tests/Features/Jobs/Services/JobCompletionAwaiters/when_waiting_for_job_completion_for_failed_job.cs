using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
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
            await NhibernateUnitOfWorkRunner.RunAsync(
                () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
                async newUnitOfWork =>
                {
                    _job = new JobBuilder(newUnitOfWork)
                        .WithGuid(_jobGuid)
                        .Build();

                    for (var i = 1; i <= RebusConstants.MaxDeliveryAttempts; i++)
                    {
                        _job.HandlingStarted(RebusQueues.Main);
                        _job.Fail(new Exception($"Error {i}"));
                    }

                    await Task.Delay(200);
                }
            );
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

        await NhibernateUnitOfWorkRunner.RunAsync(
            () => new NhibernateUnitOfWork(TestFixtureContext.NhibernateConfigurator),
            async newUnitOfWork =>
            {
                await newUnitOfWork.DeleteJobCascade(_jobGuid);
            }
        );
    }
}