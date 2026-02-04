using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_with_search_term_set : BaseTest
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
    public void watchdog_is_refreshed()
    {
        _watchdog.CurrentScrapedResults.ShouldBe(["Doom 1", "Doom 2"]);
        _watchdog.ScrapedResultsToNotifyAbout.ShouldBe(["Doom 2"]);
    }

    [Test]
    public void watchdog_scraped_results_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogScrapedResultsUpdatedDomainEvent(_watchdog.Id));
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
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1", "Another World"]);
        _scraper.EnableWebPage(scraperWebPage.Id);

        _watchdog = new WatchdogBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm("doom")
            .Build();
        
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
    }
}