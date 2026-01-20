using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_the_first_web_scraper_failing_and_the_second_succeeding
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://google.com",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden
                }))
            .Build();

        var webScraperChain = new WebScraperChain([
            new HttpClientScraper(httpClientFactory),
            new HttpClientScraper(new HttpClientFactory())
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            "https://google.com",
            scrapeHtmlAsRenderedByBrowser: false,
            httpHeaders: null
        );
    }

    [Test]
    public void scrape_results_is_correct()
    {
        _scrapeResult.Success.ShouldBe(true);
        _scrapeResult.Content.ShouldNotBeNull();
        _scrapeResult.Content.ShouldContain("<html");
        _scrapeResult.Content.Length.ShouldBeGreaterThan(10000);
        _scrapeResult.FailureReason.ShouldBe(null);
        _scrapeResult.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
    }
}