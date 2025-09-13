using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_creating_watchdog : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        var user = new UserBuilder().Build();
        _watchdog = new Watchdog(user, "watchdog name");
    }

    [Test]
    public void default_values_are_set_correctly()
    {
        _watchdog.ScrapingIntervalInSeconds.ShouldBe(Watchdog.DefaultScrapingIntervalOneDayInSeconds);
        _watchdog.IntervalBetweenSameResultAlertsInDays.ShouldBe(Watchdog.DefaultIntervalBetweenSameResultAlertsInDays);
    }
}