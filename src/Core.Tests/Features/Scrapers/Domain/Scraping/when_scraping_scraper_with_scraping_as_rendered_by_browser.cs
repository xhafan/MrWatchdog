using System.Net;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Domain.Scraping;

[TestFixture]
public class when_scraping_scraper_with_scraping_as_rendered_by_browser : BaseTest
{
    private Scraper _scraper = null!;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();

        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://google.com",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                }))
            .Build();
        await _scraper.Scrape(new WebScraperChain([
            new HttpClientScraper(httpClientFactory),
            new PlaywrightScraper(
                await RunOncePerTestRun.PlaywrightTask.Value,
                OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
            )
        ]));
    }

    [Test]
    public void scraper_web_page_is_scraped_by_playwright_scraper()
    {
        var webPage = _scraper.WebPages.Single();
        webPage.ScrapedOn.ShouldNotBeNull();
        webPage.ScrapedOn.Value.ShouldBe(DateTime.UtcNow, tolerance: TimeSpan.FromSeconds(30));
        webPage.ScrapingResults.ShouldNotBeEmpty();
        webPage.ScrapingErrorMessage.ShouldBe(null);
    }

    private void _BuildEntities()
    {
        _scraper = new ScraperBuilder()
            .WithWebPage(new ScraperWebPageArgs
            {
                Url = "https://google.com",
                Selector = "html",
                Name = "google.com",
                ScrapeHtmlAsRenderedByBrowser = true
            })
            .Build();
    }
}