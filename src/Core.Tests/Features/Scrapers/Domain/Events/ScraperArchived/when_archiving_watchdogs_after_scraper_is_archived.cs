using CoreDdd.Queries;
using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperArchived;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Events.ScraperArchived;

[TestFixture]
public class when_archiving_watchdogs_after_scraper_is_archived : BaseDatabaseTest
{
    private Scraper _scraper = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        var queryHandlerFactory = new FakeQueryHandlerFactory();
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogsForScraperQueryHandler(UnitOfWork));
        
        var handler = new ArchiveWatchdogsDomainEventMessageHandler(
            new QueryExecutor(queryHandlerFactory),
            _bus
        );

        await handler.Handle(new ScraperArchivedDomainEvent(_scraper.Id));
    }

    [Test]
    public void command_is_sent_over_message_bus_for_related_watchdog()
    {
        A.CallTo(() => _bus.Send(new ArchiveWatchdogCommand(_watchdog.Id))).MustHaveHappenedOnceExactly();
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder(UnitOfWork).Build();
            
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithScraper(_scraper)
            .Build();
    }
}