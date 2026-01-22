using System.Net;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_html_as_rendered_by_browser_with_two_web_scrapers_for_non_existent_url
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var playwrightScraper = new PlaywrightScraper(
            await RunOncePerTestRun.PlaywrightTask.Value,
            OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
        );
        var webScraperChain = new WebScraperChain([
            playwrightScraper,
            playwrightScraper
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            $"https://google.com/{Guid.NewGuid()}",
            scrapeHtmlAsRenderedByBrowser: true,
            httpHeaders: null
        );
    }

    [Test]
    public void second_web_scraper_is_not_executed()
    {
        _scrapeResult.Success.ShouldBe(false);
        _scrapeResult.FailureReason.ShouldBe(
            """
            Scraping failed:
            PlaywrightScraper: Error scraping web page, HTTP status code: 404
            """,
            ignoreLineEndings: true
        );
        _scrapeResult.Content.ShouldBeNull();
        _scrapeResult.HttpStatusCode.ShouldBe((int)HttpStatusCode.NotFound);
    }
}