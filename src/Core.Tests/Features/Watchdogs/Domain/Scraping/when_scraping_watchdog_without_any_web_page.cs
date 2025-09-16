using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Scraping;

[TestFixture]
public class when_scraping_watchdog_without_any_web_page : BaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildWatchdogWhichCan();

        var httpClientFactory = new HttpClientFactoryBuilder().Build();

        await _watchdog.Scrape(httpClientFactory);
    }

    [Test]
    public void watchdog_scraping_completed_domain_event_is_not_raised()
    {
        RaisedDomainEvents.ShouldNotContain(new WatchdogScrapingCompletedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void watchdog_cannot_notify_about_failed_scraping()
    {
        _watchdog.CanNotifyAboutFailedScraping.ShouldBe(false);
    }
    
    private void _BuildWatchdogWhichCan()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage([])
            .Build();
    }
}