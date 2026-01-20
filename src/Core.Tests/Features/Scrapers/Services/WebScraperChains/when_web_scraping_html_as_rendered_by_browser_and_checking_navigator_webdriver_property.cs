using Microsoft.Playwright;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_html_as_rendered_by_browser_and_checking_navigator_webdriver_property
{
    private IPlaywright _playwright = null!;
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        _playwright = await Playwright.CreateAsync();

        var webScraperChain = new WebScraperChain([
            new PlaywrightScraper(
                _playwright,
                OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
            )
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            "https://browserleaks.com/javascript",
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
        _scrapeResult.Content.ShouldContain(
            """
            <tr><td>webdriver</td><td id="js-webdriver">false</td></tr>
            """
        );
    }

    [TearDown]
    public void Cleanup()
    {
        _playwright.Dispose();
    }
}