using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_deleting_watchdog_search : BaseDatabaseTest
{
    private WatchdogSearch? _watchdogSearch;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new DeleteWatchdogSearchCommandMessageHandler(new NhibernateRepository<WatchdogSearch>(UnitOfWork));

        await handler.Handle(new DeleteWatchdogSearchCommand(_watchdogSearch!.Id));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogSearch = UnitOfWork.Get<WatchdogSearch>(_watchdogSearch.Id);
    }

    [Test]
    public void watchdog_search_is_deleted()
    {
        _watchdogSearch.ShouldBeNull();
    }

    private void _BuildEntities()
    {
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork).Build();
    }
}