using CoreBackend.TestsShared;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_where_previously_scraping_failed : BaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdog.Refresh();
    }

    [Test]
    public void watchdog_scraped_results_to_notify_about_is_correct()
    {
        _watchdog.ScrapedResultsToNotifyAbout.Select(x => x.Value).ShouldBeEmpty();
    }

    private void _BuildEntities()
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

        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1"]);
        _scraper.EnableWebPage(scraperWebPage.Id);
        
        _watchdog = new WatchdogBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapedResults(scraperWebPage.Id, []); // simulating scraping failure by setting empty scraped results

        _watchdog.Refresh();

        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1"]);
    }
}