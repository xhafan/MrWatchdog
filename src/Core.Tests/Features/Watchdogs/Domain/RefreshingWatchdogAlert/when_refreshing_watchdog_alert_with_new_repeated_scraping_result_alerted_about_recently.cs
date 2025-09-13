using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.RefreshingWatchdogAlert;

[TestFixture]
public class when_refreshing_watchdog_alert_with_new_repeated_scraping_result_alerted_about_recently : BaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogAlert _watchdogAlert = null!;

    [SetUp]
    public async Task Context()
    {
        await _BuildEntities();
        
        _watchdogAlert.Refresh();
    }

    [Test]
    public void watchdog_alert_scraping_results_to_alert_about_is_correct()
    {
        _watchdogAlert.ScrapingResultsToAlertAbout.ShouldBe([]);
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
            .WithIntervalBetweenSameResultAlertsInDays(30)
            .Build();
        var watchdogWebPage = _watchdog.WebPages.Single();

        _watchdogAlert = new WatchdogAlertBuilder()
            .WithWatchdog(_watchdog)
            .WithSearchTerm(null)
            .Build();
        
        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1"]);
        _watchdogAlert.Refresh();
        await _watchdogAlert.AlertUserAboutNewScrapingResults(A.Fake<IEmailSender>(), OptionsRetriever.Retrieve<RuntimeOptions>().Value);

        _watchdog.SetScrapingResults(watchdogWebPage.Id, []);
        _watchdogAlert.Refresh();

        _watchdog.SetScrapingResults(watchdogWebPage.Id, ["Doom 1"]);
        _watchdogAlert.Refresh();
    }
}