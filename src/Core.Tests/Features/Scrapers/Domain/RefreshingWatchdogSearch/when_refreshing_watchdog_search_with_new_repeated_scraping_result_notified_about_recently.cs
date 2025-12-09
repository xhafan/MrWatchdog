using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_new_repeated_scraping_result_notified_about_recently : BaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogSearch _watchdogSearch = null!;

    [SetUp]
    public async Task Context()
    {
        await _BuildEntities();
        
        _watchdogSearch.Refresh();
    }

    [Test]
    public void watchdog_search_scraping_results_to_notify_about_is_correct()
    {
        _watchdogSearch.ScrapingResultsToNotifyAbout.ShouldBe([]);
    }

    private async Task _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .WithIntervalBetweenSameResultNotificationsInDays(30)
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();

        _watchdogSearch = new WatchdogSearchBuilder()
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1"]);
        _watchdogSearch.Refresh();
        await _watchdogSearch.NotifyUserAboutNewScrapingResults(A.Fake<IEmailSender>(), OptionsTestRetriever.Retrieve<RuntimeOptions>().Value);

        _watchdog.SetScrapingResults(watchdogWebPage.Id, []);
        _watchdogSearch.Refresh();

        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1"]);
        _watchdogSearch.Refresh();
    }
}