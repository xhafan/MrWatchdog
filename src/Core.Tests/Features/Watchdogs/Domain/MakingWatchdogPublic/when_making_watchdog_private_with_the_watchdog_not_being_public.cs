using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.MakingWatchdogPublic;

[TestFixture]
public class when_making_watchdog_private_with_the_watchdog_not_being_public
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder().Build();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _watchdog.MakePrivate());

        ex.Message.ShouldBe("Watchdog is already private.");
    }
}