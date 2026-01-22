using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_html_as_rendered_by_browser_with_extra_http_headers
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var webScraperChain = new WebScraperChain([
            new PlaywrightScraper(
                await RunOncePerTestRun.PlaywrightTask.Value,
                OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
            )
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            "https://www.whatismybrowser.com/detect/what-http-headers-is-my-browser-sending/",
            scrapeHtmlAsRenderedByBrowser: true,
            httpHeaders: [("X-Test-Header", "TestValue")]
        );
    }

    [Test]
    public void scrape_results_is_correct()
    {
        _scrapeResult.FailureReason.ShouldBe(null);
        _scrapeResult.Success.ShouldBe(true);
        _scrapeResult.Content.ShouldNotBeNull();
        _scrapeResult.Content.ShouldContain("X-Test-Header");
        _scrapeResult.Content.ShouldContain("TestValue");
    }
}