using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogSearchScrapingResultsUpdated;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_search_term_set : BaseTest
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
        _watchdogSearch.CurrentScrapingResults.ShouldBe(["Doom 1", "Doom 2"]);
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe(["Doom 2"]);
    }

    [Test]
    public void watchdog_search_scraping_results_updated_domain_event_is_raised()
    {
        RaisedDomainEvents.ShouldContain(new WatchdogSearchScrapingResultsUpdatedDomainEvent(_watchdogSearch.Id));
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

        _watchdogSearch = new WatchdogSearchBuilder()
            .WithWatchdog(_watchdog)
            .WithSearchTerm("doom")
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
    }
}