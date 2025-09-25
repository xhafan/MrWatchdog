using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogAlert;

[TestFixture]
public class when_refreshing_watchdog_alert_with_search_term_set : BaseTest
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
        _watchdogAlert.CurrentScrapingResults.ShouldBe(["Doom 1", "Doom 2"]);
        _watchdogAlert.ScrapingResultsToAlertAbout.ShouldBe(["Doom 2"]);
    }

    [Test]
    public void watchdog_alert_scraping_results_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogAlertScrapingResultsUpdatedDomainEvent(_watchdogAlert.Id));
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
        _watchdog.EnableWebPage(watchdogWebPage.Id);

        _watchdogAlert = new WatchdogAlertBuilder()
            .WithWatchdog(_watchdog)
            .WithSearchTerm("doom")
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
    }
}