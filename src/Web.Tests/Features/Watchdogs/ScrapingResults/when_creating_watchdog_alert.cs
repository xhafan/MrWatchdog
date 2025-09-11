using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

[TestFixture]
public class when_creating_watchdog_alert : BaseDatabaseTest
{
    private ScrapingResultsController _controller = null!;
    private Watchdog _watchdog = null!;
    private ICoreBus _bus = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _bus = A.Fake<ICoreBus>();
        
        _controller = new ScrapingResultsControllerBuilder()
            .WithBus(_bus)
            .Build();
        
        await _controller.CreateWatchdogAlert(_watchdog.Id, " search term ");
    }

    [Test]
    public void command_is_sent_over_message_bus()
    {
        A.CallTo(() => _bus.Send(new CreateWatchdogAlertCommand(_watchdog.Id, "search term"))).MustHaveHappenedOnceExactly();
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
    }    
}