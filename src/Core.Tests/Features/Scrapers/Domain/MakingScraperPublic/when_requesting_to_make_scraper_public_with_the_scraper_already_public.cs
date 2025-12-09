using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.MakingWatchdogPublic;

[TestFixture]
public class when_requesting_to_make_watchdog_public_with_the_watchdog_already_public
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder().Build();
        _watchdog.MakePublic();
    }

    [Test]
    public void exception_is_thrown()
    {
        var ex = Should.Throw<Exception>(() => _watchdog.RequestToMakePublic());

        ex.Message.ShouldBe("Watchdog is already public.");
    }
}