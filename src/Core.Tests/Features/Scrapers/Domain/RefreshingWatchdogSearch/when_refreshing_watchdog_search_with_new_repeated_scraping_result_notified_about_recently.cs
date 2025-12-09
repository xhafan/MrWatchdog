using FakeItEasy;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.RefreshingWatchdogSearch;

[TestFixture]
public class when_refreshing_watchdog_search_with_new_repeated_scraping_result_notified_about_recently : BaseTest
{
    private Scraper _scraper = null!;
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
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .WithIntervalBetweenSameResultNotificationsInDays(30)
            .Build();
        var scraperWebPage = _scraper.WebPages.Single();

        _watchdogSearch = new WatchdogSearchBuilder()
            .WithScraper(_scraper)
            .WithSearchTerm(null)
            .Build();
        
        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1"]);
        _watchdogSearch.Refresh();
        await _watchdogSearch.NotifyUserAboutNewScrapingResults(A.Fake<IEmailSender>(), OptionsTestRetriever.Retrieve<RuntimeOptions>().Value);

        _scraper.SetScrapingResults(scraperWebPage.Id, []);
        _watchdogSearch.Refresh();

        _scraper.SetScrapingResults(scraperWebPage.Id, ["Doom 1"]);
        _watchdogSearch.Refresh();
    }
}