using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using System.Globalization;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_with_new_repeated_scraping_result_notified_about_recently : BaseTest
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
    public void watchdog_scraped_results_to_notify_about_is_correct()
    {
        _watchdog.ScrapedResultsToNotifyAbout.ShouldBe([]);
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
        
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1"]);
        _watchdog.Refresh();
        await _watchdog.NotifyUserAboutNewScrapedResults(
            CultureInfo.GetCultureInfo("en"),
            A.Fake<ICoreBus>(),
            OptionsTestRetriever.Retrieve<RuntimeOptions>().Value
        );

        _scraper.SetScrapedResults(scraperWebPage.Id, []);
        _watchdog.Refresh();

        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1"]);
        _watchdog.Refresh();
    }
}