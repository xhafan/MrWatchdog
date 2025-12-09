using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;

[TestFixture]
public class when_refreshing_watchdog_search_after_scraper_scraping_completed : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private WatchdogSearch _watchdogSearch = null!;
    private ICoreBus _bus = null!;
    private WatchdogSearch _watchdogSearchForAnotherScraper = null!;
    private WatchdogSearch _archivedWatchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogSearchesForScraperQueryHandler(UnitOfWork));
        
        var handler = new RefreshWatchdogSearchesDomainEventMessageHandler(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );

        await handler.Handle(new ScraperScrapingCompletedDomainEvent(_scraper.Id));
    }

    [Test]
    public void command_is_sent_over_message_bus_for_related_watchdog_search()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogSearchCommand(_watchdogSearch.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void command_is_not_sent_over_message_bus_for_archived_watchdog_search()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogSearchCommand(_archivedWatchdogSearch.Id))).MustNotHaveHappened();
    }
    
    [Test]
    public void command_is_not_sent_over_message_bus_for_unrelated_watchdog_search()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogSearchCommand(_watchdogSearchForAnotherScraper.Id))).MustNotHaveHappened();
    }    
    
    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork)
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _scraperWebPageId = _scraper.WebPages.Single().Id;
        _scraper.SetScrapingResults(_scraperWebPageId, ["Two Point Hospital"]);

        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();

        _archivedWatchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .Build();
        _archivedWatchdogSearch.Archive();
        
        _watchdogSearchForAnotherScraper = new WatchdogSearchBuilder(UnitOfWork).Build();        
    }
}