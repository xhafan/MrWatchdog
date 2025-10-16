using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.MakingWatchdogPublic;

[TestFixture]
public class when_requesting_to_make_watchdog_public : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder().Build();

        _watchdog.RequestToMakePublic();
    }

    [Test]
    public void public_status_is_correct()
    {
        _watchdog.PublicStatus.ShouldBe(PublicStatus.MakePublicRequested);
    }
}