using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared.HttpClients;
using System.Net;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_the_first_web_scraper_failing_on_too_large_response_and_stopping_the_web_scraping_chain
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://google.com",
                () =>
                {
                    var httpResponseMessage = new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("content")
                    };
                    httpResponseMessage.Content.Headers.ContentLength = ScrapingConstants.WebPageSizeLimitInMegaBytes * 1024 * 1024 + 1;
                    return httpResponseMessage;
                }))
            .Build();

        var webScraperChain = new WebScraperChain([
            new HttpClientScraper(httpClientFactory),
            new HttpClientScraper(httpClientFactory)
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
        _scrapeResult.Success.ShouldBe(false);
        _scrapeResult.Content.ShouldBe(null);
        _scrapeResult.FailureReason.ShouldBe(
            """
            Scraping failed:
            HttpClientScraper: Web page https://google.com larger than 10 MB.
            """,
            ignoreLineEndings: true
        );
    }
}