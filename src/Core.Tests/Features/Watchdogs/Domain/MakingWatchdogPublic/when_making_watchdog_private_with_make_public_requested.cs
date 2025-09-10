using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.MakingWatchdogPublic;

[TestFixture]
public class when_making_watchdog_private_with_make_public_requested
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder().Build();
        _watchdog.RequestToMakePublic();

        _watchdog.MakePrivate();
    }

    [Test]
    public void make_public_requested_flag_is_reset()
    {
        _watchdog.MakePublicRequested.ShouldBe(false);
    }
}