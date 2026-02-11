using System.Globalization;
using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain;

[TestFixture]
public class when_notifying_user_about_new_scraped_results : BaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        await _watchdog.NotifyUserAboutNewScrapedResults(
            CultureInfo.GetCultureInfo("en"),
            A.Fake<ICoreBus>(),
            OptionsTestRetriever.Retrieve<RuntimeOptions>().Value
        );
    }

    [Test]
    public void watchdog_scraping_result_history_is_populated()
    {
        _watchdog.ScrapedResultsHistory.Count().ShouldBe(1);
        var scrapedResultHistory = _watchdog.ScrapedResultsHistory.SingleOrDefault(x => x.Result == "Doom 2");
        scrapedResultHistory.ShouldNotBeNull();
        scrapedResultHistory.NotifiedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
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
        _watchdog.Refresh();
    }
}