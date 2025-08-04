using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_updating_watchdog_alert_overview : BaseDatabaseTest
{
    private WatchdogAlert _watchdogAlert = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new UpdateWatchdogAlertOverviewCommandMessageHandler(new NhibernateRepository<WatchdogAlert>(UnitOfWork));

        await handler.Handle(new UpdateWatchdogAlertOverviewCommand(new WatchdogAlertOverviewArgs
        {
            WatchdogAlertId = _watchdogAlert.Id,
            SearchTerm = "updated search term"
        }));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogAlert = UnitOfWork.LoadById<WatchdogAlert>(_watchdogAlert.Id);
    }

    [Test]
    public void watchdog_alert_overview_is_updated()
    {
        _watchdogAlert.SearchTerm.ShouldBe("updated search term");
    }

    private void _BuildEntities()
    {
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork)
            .WithSearchTerm("search term")
            .Build();
    }
}