using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands;

[TestFixture]
public class when_updating_watchdog_search_overview : BaseDatabaseTest
{
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        var handler = new UpdateWatchdogSearchOverviewCommandMessageHandler(new NhibernateRepository<WatchdogSearch>(UnitOfWork));

        await handler.Handle(new UpdateWatchdogSearchOverviewCommand(new WatchdogSearchOverviewArgs
        {
            WatchdogSearchId = _watchdogSearch.Id,
            SearchTerm = "updated search term"
        }));
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogSearch = UnitOfWork.LoadById<WatchdogSearch>(_watchdogSearch.Id);
    }

    [Test]
    public void watchdog_search_overview_is_updated()
    {
        _watchdogSearch.SearchTerm.ShouldBe("updated search term");
    }

    private void _BuildEntities()
    {
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithSearchTerm("search term")
            .Build();
    }
}