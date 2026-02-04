using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;

[TestFixture]
public class when_refreshing_watchdog_after_scraper_scraping_completed : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private long _scraperWebPageId;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;
    private Watchdog _watchdogForAnotherScraper = null!;
    private Watchdog _archivedWatchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogsForScraperQueryHandler(UnitOfWork));
        
        var handler = new RefreshWatchdogsDomainEventMessageHandler(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );

        await handler.Handle(new ScraperScrapingCompletedDomainEvent(_scraper.Id));
    }

    [Test]
    public void command_is_sent_over_message_bus_for_related_watchdog()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogCommand(_watchdog.Id))).MustHaveHappenedOnceExactly();
    }

    [Test]
    public void command_is_not_sent_over_message_bus_for_archived_watchdog()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogCommand(_archivedWatchdog.Id))).MustNotHaveHappened();
    }
    
    [Test]
    public void command_is_not_sent_over_message_bus_for_unrelated_watchdog()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogCommand(_watchdogForAnotherScraper.Id))).MustNotHaveHappened();
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
        _scraper.SetScrapedResults(_scraperWebPageId, ["Two Point Hospital"]);

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();

        _archivedWatchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .Build();
        _archivedWatchdog.Archive();
        
        _watchdogForAnotherScraper = new WatchdogBuilder(UnitOfWork).Build();        
    }
}