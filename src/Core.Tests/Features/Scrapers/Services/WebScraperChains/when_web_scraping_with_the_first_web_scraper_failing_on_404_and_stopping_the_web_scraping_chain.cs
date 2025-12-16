using System.Net;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_the_first_web_scraper_failing_on_404_and_stopping_the_web_scraping_chain
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
                    StatusCode = HttpStatusCode.NotFound
                }))
            .Build();

        var webScraperChain = new WebScraperChain([
            new HttpClientScraper(httpClientFactory),
            new CurlScraper()
        ]);

        _scrapeResult = await webScraperChain.Scrape("https://google.com", httpHeaders: null);
    }

    [Test]
    public void scrape_results_is_correct()
    {
        _scrapeResult.Success.ShouldBe(false);
        _scrapeResult.Content.ShouldBe(null);
        _scrapeResult.FailureReason.ShouldBe(
            """
            Scraping failed:
            HttpClientScraper: Error scraping web page, HTTP status code: 404 Not Found
            """
        );
    }
}