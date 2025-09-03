using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.MakingWatchdogPublic;

[TestFixture]
public class when_making_watchdog_private
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder().Build();
        _watchdog.MakePublic();

        _watchdog.MakePrivate();
    }

    [Test]
    public void public_flag_is_reset()
    {
        _watchdog.Public.ShouldBe(false);
    }
}