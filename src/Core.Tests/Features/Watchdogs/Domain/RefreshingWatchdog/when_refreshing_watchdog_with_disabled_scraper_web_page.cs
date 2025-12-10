using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_with_disabled_scraper_web_page : BaseTest
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
    public void watchdog_search_is_refreshed()
    {
        _watchdog.CurrentScrapingResults.ShouldBe([]);
        _watchdog.ScrapingResultsToNotifyAbout.ShouldBe([]);
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
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _updateScraperWebPageInOrderToDisableIt();

        _watchdog = new WatchdogBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1", "Another World"]);
        return;

        void _updateScraperWebPageInOrderToDisableIt()
        {
            _scraper.UpdateWebPage(new ScraperWebPageArgs
            {
                ScraperWebPageId = scraperWebPage.Id,
                Url = "http://url.com/page",
                Selector = ".invalid_selector",
                Name = "url.com/page"
            });
        }

    }
}