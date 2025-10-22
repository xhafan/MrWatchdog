using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;

[TestFixture]
public class when_refreshing_watchdog_search_after_watchdog_scraping_completed : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
    private WatchdogSearch _watchdogSearch = null!;
    private ICoreBus _bus = null!;
    private WatchdogSearch _watchdogSearchForAnotherWatchdog = null!;
    private WatchdogSearch _archivedWatchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogSearchesForWatchdogQueryHandler(UnitOfWork));
        
        var handler = new RefreshWatchdogSearchesDomainEventMessageHandler(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );

        await handler.Handle(new WatchdogScrapingCompletedDomainEvent(_watchdog.Id));
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
        A.CallTo(() => _bus.Send(new RefreshWatchdogSearchCommand(_watchdogSearchForAnotherWatchdog.Id))).MustNotHaveHappened();
    }    
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "https://www.pcgamer.com/epic-games-store-free-games-list/",
                Selector = """
                           div#article-body p.infoUpdate-log a[href^="https://store.epicgames.com/"]
                           """,
                Name = "www.pcgamer.com/epic-games-store-free-games-list/"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingResults(_watchdogWebPageId, ["Two Point Hospital"]);

        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();

        _archivedWatchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .Build();
        _archivedWatchdogSearch.Archive();
        
        _watchdogSearchForAnotherWatchdog = new WatchdogSearchBuilder(UnitOfWork).Build();        
    }
}