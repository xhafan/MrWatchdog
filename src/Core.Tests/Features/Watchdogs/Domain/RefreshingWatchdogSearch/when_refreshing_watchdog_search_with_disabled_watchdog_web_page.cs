using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_disabled_watchdog_web_page : BaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdogSearch.Refresh();
    }

    [Test]
    public void watchdog_search_is_refreshed()
    {
        _watchdogSearch.CurrentScrapingResults.ShouldBe([]);
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe([]);
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();
        _updateWatchdogWebPageInOrderToDisableIt();

        _watchdogSearch = new WatchdogSearchBuilder()
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Another World"]);
        return;

        void _updateWatchdogWebPageInOrderToDisableIt()
        {
            _watchdog.UpdateWebPage(new WatchdogWebPageArgs
            {
                WatchdogWebPageId = watchdogWebPage.Id,
                Url = "http://url.com/page",
                Selector = ".invalid_selector",
                Name = "url.com/page"
            });
        }

    }
}