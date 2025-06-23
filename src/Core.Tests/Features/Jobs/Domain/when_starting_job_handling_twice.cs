using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Jobs.Domain;

[TestFixture]
public class when_starting_job_handling_twice
{
    private Job _job = null!;

    [SetUp]
    public void Context()
    {
        _job = new JobBuilder().Build();
        
        _job.HandlingStarted();
        
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _job.HandlingStarted());
        
        ex.Message.ShouldBe("Job handling already started.");
    }
}