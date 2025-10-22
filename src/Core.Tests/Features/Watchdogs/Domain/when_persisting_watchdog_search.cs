using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_persisting_watchdog_search : BaseDatabaseTest
{
    private WatchdogSearch _newWatchdogSearch = null!;
    private WatchdogSearch? _persistedWatchdogSearch;
    private Watchdog _watchdog = null!;
    private User _user = null!;

    [SetUp]
    public void Context()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPageId = _watchdog.WebPages.Single().Id;
        _watchdog.SetScrapingResults(watchdogWebPageId, ["<div>text</div>", "<div>hello</div>"]);
        _watchdog.EnableWebPage(watchdogWebPageId);

        _user = new UserBuilder(UnitOfWork).Build();
        
        _newWatchdogSearch = new WatchdogSearchBuilder(UnitOfWork)
            .WithWatchdog(_watchdog)
            .WithUser(_user)
            .WithSearchTerm("text")
            .Build();
        
        UnitOfWork.Flush();
        UnitOfWork.Clear();

        _persistedWatchdogSearch = UnitOfWork.Get<WatchdogSearch>(_newWatchdogSearch.Id);
    }

    [Test]
    public void persisted_watchdog_search_can_be_retrieved_and_has_correct_data()
    {
        _persistedWatchdogSearch.ShouldNotBeNull();
        _persistedWatchdogSearch.ShouldBe(_newWatchdogSearch);

        _persistedWatchdogSearch.Watchdog.ShouldBe(_watchdog);
        _persistedWatchdogSearch.User.ShouldBe(_user);
        _persistedWatchdogSearch.SearchTerm.ShouldBe("text");
        _persistedWatchdogSearch.CurrentScrapingResults.ShouldBe(["<div>text</div>"]);
        _persistedWatchdogSearch.IsArchived.ShouldBe(false);
    }
}