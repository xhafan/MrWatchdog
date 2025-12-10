using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_disabled_scraper_web_page : BaseTest
{
    private Scraper _scraper = null!;
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdogSearch.Refresh();
    }

    [Test]
    public void watchdog_search_is_refreshed()
    {
        _watchdogSearch.CurrentScrapingResults.ShouldBe([]);
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe([]);
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

        _watchdogSearch = new WatchdogSearchBuilder()
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