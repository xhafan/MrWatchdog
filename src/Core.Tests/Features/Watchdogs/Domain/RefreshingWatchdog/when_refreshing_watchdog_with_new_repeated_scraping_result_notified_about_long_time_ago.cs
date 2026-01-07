using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_with_new_repeated_scraping_result_notified_about_long_time_ago : BaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        await _BuildEntities();
        
        _watchdog.Refresh();
    }

    [Test]
    public void watchdog_search_scraping_results_to_notify_about_is_correct()
    {
        _watchdog.ScrapingResultsToNotifyAbout.ShouldBe(["Doom 1"]);
    }

    private async Task _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .WithIntervalBetweenSameResultNotificationsInDays(30)
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();

        _watchdog = new WatchdogBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        _watchdog.Refresh();
        
        await _simulateNotifyingUserAboutTheSameScrapingResultLongTimeAgo();

        async Task _simulateNotifyingUserAboutTheSameScrapingResultLongTimeAgo()
        {
            Clock.CurrentDateTimeProvider.Value = () => DateTime.Now.AddDays(-31);
            await _watchdog.NotifyUserAboutNewScrapingResults(A.Fake<ICoreBus>(), OptionsTestRetriever.Retrieve<RuntimeOptions>().Value);
            Clock.CurrentDateTimeProvider.Value = null;
        }

        _scraper.SetScrapingResults(scraperWebPage.Id, []);
        _watchdog.Refresh();

        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1"]);
        _watchdog.Refresh();
    }
}