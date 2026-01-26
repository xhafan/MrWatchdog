using CoreUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class PlaywrightScraper(
    IPlaywright playwright,
    IOptions<PlaywrightScraperOptions> playwrightScraperOptions,
    ILogger<PlaywrightScraper>? logger = null
) : IWebScraper
{
    public int Priority => 30;
    public bool IsBrowserRenderedHtmlScrapingSupported => true;

    public async Task<ScrapeResult> Scrape(
        string url, 
        ICollection<(string Name, string Value)>? httpHeaders,
        ScrapeOptions? options = null
    )
    {
        var isPageTooLarge = false;

        try
        {
            var launchOptions = new BrowserTypeLaunchOptions 
            {
                ExecutablePath = playwrightScraperOptions.Value.BrowserExecutablePath,
                Headless = false,
                Args = ["--window-position=-2000,0"] // Open the browser window "Off-Screen"
            };
            
            await using var browser = await playwright.Chromium.LaunchAsync(launchOptions);

            var browserContext = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                ExtraHTTPHeaders = httpHeaders?.ToDictionary(x => x.Name, x => x.Value)
            });

            await browserContext.AddInitScriptAsync(
                """
                Object.defineProperty(navigator, 'webdriver', {
                  get: () => false
                });
                """
            );

            var page = await browserContext.NewPageAsync();

            _checkContentLengthResponseSize();
            await _checkChunkEncodingResponseSize();

            var waitUntil = _MapWaitFor(options?.ScrapingByBrowserWaitFor); // not unit tested

            var response = await page.GotoAsync(url, new PageGotoOptions {WaitUntil = waitUntil});
            Guard.Hope(response != null, "Response is null.");

            if (!response.Ok)
            {
                return ScrapeResult.Failed(
                    $"Error scraping web page, HTTP status code: {response.Status}",
                    httpStatusCode: response.Status
                );
            }

            var html = await page.ContentAsync();

            return ScrapeResult.Succeeded(
                html,
                httpStatusCode: response.Status
            );

            void _checkContentLengthResponseSize() // not unit tested
            {
                page.Response += async (_, pageResponse) =>
                {
                    if (pageResponse.Headers.TryGetValue("content-length", out var lengthStr)
                        && long.TryParse(lengthStr, out var length) 
                        && length > ScrapingConstants.WebPageSizeLimitInMegaBytes * 1024 * 1024)
                    {
                        isPageTooLarge = true;
                        await page.CloseAsync();
                    }
                };
            }

            async Task _checkChunkEncodingResponseSize() // not unit tested
            {
                var client = await page.Context.NewCDPSessionAsync(page);
                await client.SendAsync("Network.enable");

                long totalBytes = 0;
                string? mainDocumentRequestId = null;

                // 1. Identify the Main Document and check for Chunked Encoding
                client.Event("Network.responseReceived").OnEvent += (_, e) =>
                {
                    if (e.HasValue 
                        && e.Value.TryGetProperty("type", out var typeProp) 
                        && typeProp.GetString() == "Document") // This isolates the main page
                    {
                        if (e.Value.TryGetProperty("response", out var responseElement) 
                            && responseElement.TryGetProperty("headers", out var headers))
                        {
                            var isChunked = false;
                            foreach (var header in headers.EnumerateObject())
                            {
                                if (header.Name.Equals("transfer-encoding", StringComparison.OrdinalIgnoreCase) 
                                    && header.Value.GetString()?.Contains("chunked", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    isChunked = true;
                                    break;
                                }
                            }

                            if (isChunked && e.Value.TryGetProperty("requestId", out var idProp))
                            {
                                mainDocumentRequestId = idProp.GetString();
                            }
                        }
                    }
                };

                // 2. Monitor bytes only for that specific RequestId
                client.Event("Network.dataReceived").OnEvent += async (_, e) =>
                {
                    if (mainDocumentRequestId != null 
                        && e.HasValue 
                        && e.Value.TryGetProperty("requestId", out var idProp) 
                        && idProp.GetString() == mainDocumentRequestId)
                    {
                        if (e.Value.TryGetProperty("dataLength", out var lengthProp))
                        {
                            var currentTotal = Interlocked.Add(ref totalBytes, lengthProp.GetInt64());

                            if (currentTotal > ScrapingConstants.WebPageSizeLimitInMegaBytes * 1024 * 1024)
                            {
                                // Set ID to null to stop further processing of this request
                                mainDocumentRequestId = null;

                                isPageTooLarge = true;
                                await page.CloseAsync();
                            }
                        }
                    }
                };
            }
        }
        catch (PlaywrightException) when (isPageTooLarge)
        {
            return ScrapeResult.Failed($"Web page {url} larger than {ScrapingConstants.WebPageSizeLimitInMegaBytes} MB.", stopWebScraperChain: true);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, ex.Message);
            return ScrapeResult.Failed(ex.Message, stopWebScraperChain: true);
        }
    }

    private WaitUntilState _MapWaitFor(ScrapingByBrowserWaitFor? value) =>
        value switch
        {
            ScrapingByBrowserWaitFor.DomContentLoaded => WaitUntilState.DOMContentLoaded,
            ScrapingByBrowserWaitFor.NetworkIdle => WaitUntilState.NetworkIdle,
            _ => WaitUntilState.Load
        };
}