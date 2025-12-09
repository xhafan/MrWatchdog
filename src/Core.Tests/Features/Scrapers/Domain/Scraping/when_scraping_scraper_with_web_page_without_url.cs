using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Watchdogs.Domain.Scraping;

[TestFixture]
public class when_scraping_watchdog_with_web_page_without_url : BaseTest
{
    private Watchdog _watchdog = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var httpClientFactory = new HttpClientFactoryBuilder().Build();
        
        await _watchdog.Scrape(httpClientFactory);
    }

    [Test]
    public void watchdog_web_page_is_not_scraped()
    {
        var webPage = _watchdog.WebPages.Single();
        webPage.ScrapingResults.ShouldBeEmpty();
        webPage.ScrapedOn.ShouldBe(null);
        webPage.ScrapingErrorMessage.ShouldBe(null);
    }
    
    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder()
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = null,
                Selector = null,
                Name = null
            })
            .Build();
    }
}