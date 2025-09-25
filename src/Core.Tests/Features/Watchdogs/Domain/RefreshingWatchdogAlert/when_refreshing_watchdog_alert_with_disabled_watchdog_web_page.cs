using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogAlert;

[TestFixture]
public class when_refreshing_watchdog_alert_with_disabled_watchdog_web_page : BaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogAlert _watchdogAlert = null!;

    [SetUp]
    public void Context()
    {
        _BuildEntities();
        
        _watchdogAlert.Refresh();
    }

    [Test]
    public void watchdog_alert_is_refreshed()
    {
        _watchdogAlert.CurrentScrapingResults.ShouldBe([]);
        _watchdogAlert.ScrapingResultsToAlertAbout.ShouldBe([]);
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

        _watchdogAlert = new WatchdogAlertBuilder()
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