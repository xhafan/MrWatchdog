using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_refreshing_watchdog_with_search_term_empty : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new RefreshWatchdogCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork)
        );

        await handler.Handle(new RefreshWatchdogCommand(_watchdog.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
    }

    [Test]
    public void watchdog_is_refreshed()
    {
        _watchdog.CurrentScrapedResults.ShouldBe(["Doom 1", "Prince Of Persia"]);
        _watchdog.ScrapedResultsToNotifyAbout.ShouldBe(["Prince Of Persia"]);
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
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Another World", "Doom 1"]);
        _scraper.EnableWebPage(scraperWebPage.Id);

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapedResults(scraperWebPage.Id, ["Doom 1", "Prince Of Persia"]);
    }
}