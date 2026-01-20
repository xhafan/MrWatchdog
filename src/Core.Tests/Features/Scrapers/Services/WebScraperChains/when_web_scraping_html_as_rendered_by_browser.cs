using System.Net;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_html_as_rendered_by_browser
{
    private IPlaywright _playwright = null!;
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        _playwright = await Playwright.CreateAsync();

        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://google.com",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                }))
            .Build();


        var webScraperChain = new WebScraperChain([
            new HttpClientScraper(httpClientFactory),
            new PlaywrightScraper(
                _playwright,
                OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
            )
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            "https://google.com",
            scrapeHtmlAsRenderedByBrowser: true,
            httpHeaders: null
        );
    }

    [Test]
    public void scrape_results_is_correct()
    {
        _scrapeResult.FailureReason.ShouldBe(null);
        _scrapeResult.Success.ShouldBe(true);
        _scrapeResult.Content.ShouldNotBeNull();
        _scrapeResult.Content.ShouldContain("<html");
        _scrapeResult.Content.Length.ShouldBeGreaterThan(10000);
        _scrapeResult.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
    }

    [TearDown]
    public void Cleanup()
    {
        _playwright.Dispose();
    }
}