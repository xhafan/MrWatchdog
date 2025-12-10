using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_new_scraping_results_already_in_scraping_results_to_notify_about : BaseTest
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
    public void watchdog_search_scraping_results_to_notify_about_does_not_contain_duplicates()
    {
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe(["Doom 2"]);
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
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1", "Another World"]);
        _scraper.EnableWebPage(scraperWebPage.Id);

        _watchdogSearch = new WatchdogSearchBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
        _watchdogSearch.Refresh();
        _scraper.SetScrapingResults(scraperWebPage.Id, []);
        _watchdogSearch.Refresh();
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 2"]);
    }
}