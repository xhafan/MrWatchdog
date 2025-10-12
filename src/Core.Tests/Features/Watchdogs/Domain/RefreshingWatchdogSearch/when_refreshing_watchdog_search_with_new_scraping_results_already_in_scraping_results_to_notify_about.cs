using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_new_scraping_results_already_in_scraping_results_to_notify_about : BaseTest
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
    public void watchdog_search_scraping_results_to_notify_about_does_not_contain_duplicates()
    {
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe(["Doom 2"]);
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
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1", "Doom 2", "Another World"]);
        _watchdogSearch.Refresh();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, []);
        _watchdogSearch.Refresh();
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 2"]);
    }
}