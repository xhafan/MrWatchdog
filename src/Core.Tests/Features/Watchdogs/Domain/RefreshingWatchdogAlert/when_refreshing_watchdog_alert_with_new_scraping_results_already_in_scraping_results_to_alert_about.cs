using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogAlert;

[TestFixture]
public class when_refreshing_watchdog_alert_with_new_scraping_results_already_in_scraping_results_to_alert_about : BaseTest
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
    public void watchdog_alert_scraping_results_to_alert_about_does_not_contain_duplicates()
    {
        _watchdogAlert.ScrapingResultsToAlertAbout.ShouldBe(["Doom 2"]);
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
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Another World"]);

        _watchdogAlert = new WatchdogAlertBuilder()
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
        _watchdogAlert.Refresh();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, []);
        _watchdogAlert.Refresh();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 2"]);
    }
}