using System.Net;
using MrWatchdog.Core.Features.Scrapers.Services;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_curl_scraper_failing_on_404
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var webScraperChain = new WebScraperChain([
            new CurlScraper()
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            $"https://google.com/{Guid.NewGuid()}",
            scrapeHtmlAsRenderedByBrowser: false,
            httpHeaders: null
        );
    }

    [Test]
    public void scrape_results_is_correct()
    {
        _scrapeResult.Success.ShouldBe(false);
        _scrapeResult.Content.ShouldBe(null);
        _scrapeResult.FailureReason.ShouldNotBeNull();
        _scrapeResult.FailureReason.ShouldContain("Scraping failed:");
        _scrapeResult.FailureReason.ShouldContain("CurlScraper: Status code 404");
        _scrapeResult.HttpStatusCode.ShouldBe((int)HttpStatusCode.NotFound);
    }
}