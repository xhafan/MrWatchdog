using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_refreshing_watchdog_search_with_search_term_empty : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new RefreshWatchdogSearchCommandMessageHandler(
            new NhibernateRepository<WatchdogSearch>(UnitOfWork)
        );

        await handler.Handle(new RefreshWatchdogSearchCommand(_watchdogSearch.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        _watchdogSearch = UnitOfWork.LoadById<WatchdogSearch>(_watchdogSearch.Id);
    }

    [Test]
    public void watchdog_search_is_refreshed()
    {
        _watchdogSearch.CurrentScrapingResults.ShouldBe(["Doom 1", "Prince Of Persia"]);
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe(["Prince Of Persia"]);
    }
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Another World", "Doom 1"]);
        _scraper.EnableWebPage(scraperWebPage.Id);

        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1", "Prince Of Persia"]);
    }
}