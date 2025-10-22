using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Events.WatchdogArchived;

[TestFixture]
public class when_archiving_watchdog_searches_after_watchdog_is_archived : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogSearch _watchdogSearch = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogSearchesForWatchdogQueryHandler(UnitOfWork));
        
        var handler = new ArchiveWatchdogSearchesDomainEventMessageHandler(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );

        await handler.Handle(new WatchdogArchivedDomainEvent(_watchdog.Id));
    }

    [Test]
    public void command_is_sent_over_message_bus_for_related_watchdog_search()
    {
        A.CallTo(() => _bus.Send(new ArchiveWatchdogSearchCommand(_watchdogSearch.Id))).MustHaveHappenedOnceExactly();
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
            
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .Build();
    }
}