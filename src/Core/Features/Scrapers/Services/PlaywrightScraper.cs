using CoreUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class PlaywrightScraper( // todo: implement response size check
    IPlaywright playwright,
    IOptions<PlaywrightScraperOptions> playwrightScraperOptions,
    ILogger<PlaywrightScraper>? logger = null
) : IWebScraper
{
    public int Priority => 30;
    public bool IsBrowserRenderedHtmlScrapingSupported => true;

    public async Task<ScrapeResult> Scrape(string url, ICollection<(string Name, string Value)>? httpHeaders)
    {
        try
        {
            var launchOptions = new BrowserTypeLaunchOptions 
            {
                ExecutablePath = playwrightScraperOptions.Value.BrowserExecutablePath,
                Headless = false
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

            var response = await page.GotoAsync(url);
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
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, ex.Message);
            return ScrapeResult.Failed(ex.Message, stopWebScraperChain: true);
        }
    }
}