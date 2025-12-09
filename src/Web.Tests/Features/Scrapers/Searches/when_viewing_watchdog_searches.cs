using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Searches;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Searches;

[TestFixture]
public class when_viewing_watchdog_searches : BaseDatabaseTest
{
    private WatchdogSearch _watchdogSearchForUserOne = null!;
    private SearchesModel _model = null!;
    private User _userOne = null!;
    private User _userTwo = null!;
    private WatchdogSearch _watchdogSearchForUserTwo = null!;
    private WatchdogSearch _archivedWatchdogSearchForUserOne = null!;
    private WatchdogSearch _watchdogSearchForArchivedWatchdogForUserOne = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        _model = new SearchesModelBuilder(UnitOfWork)
            .WithActingUser(_userOne)
            .Build();
        
        await _model.OnGet();
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogSearches.ShouldContain(
            new GetWatchdogSearchesQueryResult
            {
                WatchdogSearchId = _watchdogSearchForUserOne.Id,
                WatchdogName = "watchdog name",
                SearchTerm = "search term",
                ReceiveNotification = true
            }
        );
        var watchdogSearchIds = _model.WatchdogSearches.Select(x => x.WatchdogSearchId).ToList();
        watchdogSearchIds.ShouldNotContain(_watchdogSearchForUserTwo.Id);
        watchdogSearchIds.ShouldNotContain(_archivedWatchdogSearchForUserOne.Id);
        watchdogSearchIds.ShouldNotContain(_watchdogSearchForArchivedWatchdogForUserOne.Id);
    }    
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();
        
        var watchdog = new WatchdogBuilder(UnitOfWork)
            .WithName("watchdog name")
            .Build();

        _watchdogSearchForUserOne = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithSearchTerm("search term")
            .WithUser(_userOne)
            .WithReceiveNotification(true)
            .Build();

        _archivedWatchdogSearchForUserOne = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithUser(_userOne)
            .Build();
        _archivedWatchdogSearchForUserOne.Archive();

        _watchdogSearchForUserTwo = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(watchdog)
            .WithUser(_userTwo)
            .Build();

        var archivedWatchdog = new WatchdogBuilder(UnitOfWork).Build();
        archivedWatchdog.Archive();

        _watchdogSearchForArchivedWatchdogForUserOne = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(archivedWatchdog)
            .WithUser(_userOne)
            .Build();
    }
}