using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events;

[TestFixture]
public class when_triggering_watchdog_alert : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;
    private WatchdogAlert _watchdogAlert = null!;
    private ICoreBus _bus = null!;
    private WatchdogAlert _watchdogAlertForAnotherWatchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogAlertsForWatchdogQueryHandler(UnitOfWork));
        
        var handler = new TriggerWatchdogAlertsDomainEventMessageHandler(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );

        await handler.Handle(new WatchdogScrapingCompletedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void command_is_sent_over_message_bus_for_related_watchdog_alert()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogAlertCommand(_watchdogAlert.Id))).MustHaveHappenedOnceExactly();
    }
    
    [Test]
    public void command_is_not_sent_over_message_bus_for_unrelated_watchdog_alert()
    {
        A.CallTo(() => _bus.Send(new RefreshWatchdogAlertCommand(_watchdogAlertForAnotherWatchdog.Id))).MustNotHaveHappened();
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

        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();
        
        _watchdogAlertForAnotherWatchdog = new WatchdogAlertBuilder(UnitOfWork).Build();        
    }
}