using System.Net;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_the_all_web_scrapers_failing
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://non_existent_domain.mrwatchdog.com",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Forbidden
                }))
            .Build();

        var httpClientScraperOne = new HttpClientScraper(httpClientFactory);
        var httpClientScraperTwo = new HttpClientScraper(new HttpClientFactory());

        var webScraperChain = new WebScraperChain([
            httpClientScraperOne,
            httpClientScraperTwo
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            "https://non_existent_domain.mrwatchdog.com",
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
        _scrapeResult.FailureReason.ShouldContain("HttpClientScraper: Error scraping web page, HTTP status code: 403 Forbidden");
        _scrapeResult.FailureReason.ShouldMatch("HttpClientScraper:.*non_existent_domain.mrwatchdog.com:443.*");
        _scrapeResult.HttpStatusCode.ShouldBe(0);
    }
}