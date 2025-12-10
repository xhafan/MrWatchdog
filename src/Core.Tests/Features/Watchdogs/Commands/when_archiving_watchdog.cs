using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_archiving_watchdog : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private User _actingUser = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        var handler = new ArchiveWatchdogCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            new UserRepository(UnitOfWork)
        );

        await handler.Handle(new ArchiveWatchdogCommand(_watchdog.Id) {ActingUserId = _actingUser.Id});
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdog = UnitOfWork.LoadById<Watchdog>(_watchdog.Id);
    }

    [Test]
    public void watchdog_is_archived()
    {
        _watchdog.IsArchived.ShouldBe(true);
    }

    [Test]
    public void watchdog_archived_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogArchivedDomainEvent(_watchdog.Id));
    }

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork).Build();
        _actingUser = new UserBuilder(UnitOfWork).Build();
    }
}