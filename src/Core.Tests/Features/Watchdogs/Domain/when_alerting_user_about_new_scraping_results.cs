using FakeItEasy;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain;

[TestFixture]
public class when_alerting_user_about_new_scraping_results : BaseTest
{
    private Watchdog _watchdog = null!;
    private WatchdogAlert _watchdogAlert = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        
        await _watchdogAlert.AlertUserAboutNewScrapingResults(A.Fake<IEmailSender>(), OptionsRetriever.Retrieve<RuntimeOptions>().Value);
    }

    [Test]
    public void watchdog_alert_scraping_result_history_is_populated()
    {
        _watchdogAlert.ScrapingResultsAlertHistory.Count().ShouldBe(1);
        var scrapingResultAlertHistory = _watchdogAlert.ScrapingResultsAlertHistory.SingleOrDefault(x => x.Result == "Doom 2");
        scrapingResultAlertHistory.ShouldNotBeNull();
        scrapingResultAlertHistory.AlertedOn.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(5));
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
        _watchdogAlert.Refresh();
    }
}