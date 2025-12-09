using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchScrapingResultsUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_search_term_set : BaseTest
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
        _watchdogSearch.CurrentScrapingResults.ShouldBe(["Doom 1", "Doom 2"]);
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe(["Doom 2"]);
    }

    [Test]
    public void watchdog_search_scraping_results_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogSearchScrapingResultsUpdatedDomainEvent(_watchdogSearch.Id));
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
            .WithSearchTerm("doom")
            .Build();
        
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
    }
}