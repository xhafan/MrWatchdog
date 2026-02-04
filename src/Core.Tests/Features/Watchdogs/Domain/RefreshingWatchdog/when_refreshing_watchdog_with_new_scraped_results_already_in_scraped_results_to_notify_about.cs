using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdog;

[TestFixture]
public class when_refreshing_watchdog_with_new_scraped_results_already_in_scraped_results_to_notify_about : BaseTest
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
    public void watchdog_scraped_results_to_notify_about_does_not_contain_duplicates()
    {
        _watchdog.ScrapedResultsToNotifyAbout.ShouldBe(["Doom 2"]);
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
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
        _watchdog.Refresh();
        _scraper.SetScrapedResults(scraperWebPage.Id, []);
        _watchdog.Refresh();
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 2"]);
    }
}