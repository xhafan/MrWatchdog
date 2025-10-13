using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Commands.CreatingWatchdogSearch;

[TestFixture]
public class when_creating_watchdog_search : BaseDatabaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogSearch? _watchdogSearch;
    private User _user = null!;

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

        await handler.Handle(new CreateWatchdogSearchCommand(_watchdog.Id, SearchTerm: "text") { ActingUserId = _user.Id});
        
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();
        
        _watchdogSearch = UnitOfWork.Session!.Query<WatchdogSearch>()
            .SingleOrDefault(x => x.Watchdog == _watchdog);
    }

    [Test]
    public void new_watchdog_search_is_created_with_matching_current_scraping_results()
    {
        _watchdogSearch.ShouldNotBeNull();
        _watchdogSearch.User.ShouldBe(_user);
        _watchdogSearch.ReceiveNotification.ShouldBe(true);
        _watchdogSearch.SearchTerm.ShouldBe("text");
        _watchdogSearch.CurrentScrapingResults.ShouldBe(["<div>tÉxt 1</div>", "<div>texŤ 3</div>"]);
    }
    
    private void _BuildEntities()
    {
        _user = new UserBuilder(UnitOfWork).Build();

        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["<div>tÉxt 1</div>", "<div>string 2</div>", "<div>texŤ 3</div>"]);
        _watchdog.EnableWebPage(watchdogWebPage.Id);
    }
}