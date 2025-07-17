using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Jobs.Domain;

[TestFixture]
public class when_completing_already_completed_job
{
    private readonly Guid _jobGuid = Guid.NewGuid();

    private Job _job = null!;

    [SetUp]
    public void Context()
    {
        _job = new JobBuilder()
            .WithGuid(_jobGuid)
            .Build();
        _job.HandlingStarted();
        _job.Complete();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _job.Complete());
        
        ex.Message.ShouldBe("Job has already completed.");
    }
}