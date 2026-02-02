using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_disabling_watchdog_notification : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        var handler = new DisableWatchdogNotificationCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork)
        );

        await handler.Handle(new DisableWatchdogNotificationCommand(_watchdog.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
    }

    [Test]
    public void watchdog_cannot_receive_notifications()
    {
        _watchdog.ReceiveNotification.ShouldBe(false);
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithReceiveNotification(true)
            .Build();
    }
}