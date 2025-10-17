using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_updating_watchdog_overview
{
    private Watchdog _watchdog = null!;
    private readonly DateTime _nextScrapingOn = DateTime.UtcNow;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdog.UpdateOverview(new WatchdogOverviewArgs
        {
            WatchdogId = _watchdog.Id,
            Name = "watchdog name",
            ScrapingIntervalInSeconds = 30,
            IntervalBetweenSameResultNotificationsInDays = 2.34,
            NumberOfFailedScrapingAttemptsBeforeAlerting = 5
        });
    }

    [Test]
    public void watchdog_next_scraping_on_is_adjusted()
    {
        _watchdog.NextScrapingOn.ShouldBe(_nextScrapingOn.AddSeconds(-60 + 30));
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithScrapingIntervalInSeconds(60)
            .WithNextScrapingOn(_nextScrapingOn)
            .Build();
    }
}