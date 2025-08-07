using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_deleting_watchdog_alert : BaseDatabaseTest
{
    private WatchdogAlert? _watchdogAlert;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new DeleteWatchdogAlertCommandMessageHandler(new NhibernateRepository<WatchdogAlert>(UnitOfWork));

        await handler.Handle(new DeleteWatchdogAlertCommand(_watchdogAlert!.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogAlert = UnitOfWork.Get<WatchdogAlert>(_watchdogAlert.Id);
    }

    [Test]
    public void watchdog_alert_is_deleted()
    {
        _watchdogAlert.ShouldBeNull();
    }

    private void _BuildEntities()
    {
        _watchdogAlert = new WatchdogAlertBuilder(UnitOfWork).Build();
    }
}