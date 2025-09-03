using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.MakingWatchdogPublic;

[TestFixture]
public class when_requesting_to_make_watchdog_public
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder().Build();

        _watchdog.RequestToMakePublic();
    }

    [Test]
    public void make_public_requested_flag_is_set()
    {
        _watchdog.MakePublicRequested.ShouldBe(true);
    }
}