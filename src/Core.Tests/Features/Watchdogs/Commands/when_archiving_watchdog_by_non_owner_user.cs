using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchArchived;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Commands;

[TestFixture]
public class when_archiving_watchdog_search_by_non_owner_user : BaseDatabaseTest
{
    private WatchdogSearch _watchdogSearch = null!;
    private User _actingUser = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new ArchiveWatchdogSearchCommandMessageHandler(
            new NhibernateRepository<WatchdogSearch>(UnitOfWork),
            new UserRepository(UnitOfWork)
        );

        await handler.Handle(new ArchiveWatchdogSearchCommand(_watchdogSearch.Id)
        {
            ActingUserId = _actingUser.Id
        });
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogSearch = UnitOfWork.LoadById<WatchdogSearch>(_watchdogSearch.Id);
    }

    [Test]
    public void watchdog_search_is_archived()
    {
        _watchdogSearch.IsArchived.ShouldBe(true);
    }

    [Test]
    public void watchdog_search_archived_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogSearchArchivedDomainEvent(_watchdogSearch.Id));
    }

    private void _BuildEntities()
    {
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork).Build();
        _actingUser = new UserBuilder(UnitOfWork).Build();
    }
}