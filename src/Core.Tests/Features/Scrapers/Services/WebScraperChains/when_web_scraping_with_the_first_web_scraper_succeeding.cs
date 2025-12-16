using System.Net;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_the_first_web_scraper_succeeding
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://www.pcgamer.com/epic-games-store-free-games-list/",
                () => new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(
                        """
                        <html>
                        <body>
                        <div id="article-body">
                        <p class="infoUpdate-log">
                        <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
                        </p>
                        </div>
                        </body>
                        </html>
                        """)
                }))
            .Build();

        var webScraperChain = new WebScraperChain([
            new CurlScraper(),
            new HttpClientScraper(httpClientFactory),
        ]);

        _scrapeResult = await webScraperChain.Scrape("https://www.pcgamer.com/epic-games-store-free-games-list/", httpHeaders: null);
    }

    [Test]
    public void scrape_results_is_correct()
    {
        _scrapeResult.Success.ShouldBe(true);
        _scrapeResult.Content.ShouldBe(
            """
            <html>
            <body>
            <div id="article-body">
            <p class="infoUpdate-log">
            <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
            </p>
            </div>
            </body>
            </html>
            """
        );
        _scrapeResult.FailureReason.ShouldBe(null);
    }
}