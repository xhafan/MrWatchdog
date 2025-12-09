using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.CreatingWatchdogSearch;

[TestFixture]
public class when_creating_watchdog_search_with_the_same_search_for_another_user : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogSearch? _watchdogSearch;
    private User _userOne = null!;
    private User _userTwo = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        var handler = new CreateWatchdogSearchCommandMessageHandler(
            new NhibernateRepository<Watchdog>(UnitOfWork),
            new NhibernateRepository<WatchdogSearch>(UnitOfWork),
            new UserRepository(UnitOfWork),
            UnitOfWork
        );

        await handler.Handle(new CreateWatchdogSearchCommand(_watchdog.Id, SearchTerm: "text") { ActingUserId = _userOne.Id});
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogSearch = UnitOfWork.Session!.Query<WatchdogSearch>()
            .SingleOrDefault(x => x.Watchdog == _watchdog && x.User == _userOne);
    }

    [Test]
    public void new_watchdog_search_is_created()
    {
        _watchdogSearch.ShouldNotBeNull();
        _watchdogSearch.User.ShouldBe(_userOne);
        _watchdogSearch.SearchTerm.ShouldBe("text");
    }
    
    private void _BuildEntities()
    {
        _userOne = new UserBuilder(UnitOfWork).Build();
        _userTwo = new UserBuilder(UnitOfWork).Build();

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>text 1</div>", "<div>string 2</div>", "<div>text 3</div>"]);
        
        _watchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithSearchTerm("text")
            .WithUser(_userTwo)
            .Build();
    }
}