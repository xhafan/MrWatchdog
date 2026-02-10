using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.HttpClients;

namespace MrWatchdog.Core.Tests.Features.Scrapers.Services.WebScraperChains;

[TestFixture]
public class when_web_scraping_with_the_first_web_scraper_succeeding_and_and_getting_deflate_encoding
{
    private ScrapeResult _scrapeResult = null!;

    [SetUp]
    public async Task Context()
    {
        var httpClientFactory = new HttpClientFactoryBuilder()
            .WithRequestResponse(new HttpMessageRequestResponse(
                "https://www.pcgamer.com/epic-games-store-free-games-list/",
                () =>
                {
                    var original = """
                                   <html>
                                   <body>
                                   <div id="article-body">
                                   <p class="infoUpdate-log">
                                   <a href="https://store.epicgames.com/en-US/p/two-point-hospital" target="_blank">Two Point Hospital</a>
                                   </p>
                                   </div>
                                   </body>
                                   </html>
                                   """;

                    var bytes = Encoding.UTF8.GetBytes(original);
                    using var ms = new MemoryStream();
                    using (var deflateStream = new DeflateStream(ms, CompressionMode.Compress, leaveOpen: true))
                    {
                        deflateStream.Write(bytes, 0, bytes.Length);
                    }
                    var compressed = ms.ToArray();

                    var byteArrayContent = new ByteArrayContent(compressed);
                    byteArrayContent.Headers.ContentEncoding.Add("deflate");
                    byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("text/html")
                    {
                        CharSet = "utf-8"
                    };

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = byteArrayContent
                    };
                }))
            .Build();

        var webScraperChain = new WebScraperChain([
            new PlaywrightScraper(
                await RunOncePerTestRun.PlaywrightTask.Value,
                OptionsTestRetriever.Retrieve<PlaywrightScraperOptions>()
            ),
            new HttpClientScraper(httpClientFactory),
        ]);

        _scrapeResult = await webScraperChain.Scrape(
            "https://www.pcgamer.com/epic-games-store-free-games-list/",
            scrapeHtmlAsRenderedByBrowser: false,
            httpHeaders: null
        );
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
        _scrapeResult.HttpStatusCode.ShouldBe((int)HttpStatusCode.OK);
    }
}