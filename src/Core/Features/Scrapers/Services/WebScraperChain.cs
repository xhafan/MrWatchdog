using CoreUtils;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class WebScraperChain(
    IEnumerable<IWebScraper> scrapers,
    ILogger<WebScraperChain>? logger = null
) : IWebScraperChain
{
    public async Task<ScrapeResult> Scrape(
        string url, 
        bool scrapeHtmlAsRenderedByBrowser,
        ICollection<(string Name, string Value)>? httpHeaders
    )
    {
        var scrapersOrderedByPriority = scrapers
            .Where(x => !scrapeHtmlAsRenderedByBrowser || x.IsBrowserRenderedHtmlScrapingSupported)
            .OrderBy(s => s.Priority)
            .ToList();

        var failureReasons = new List<string>();

        ScrapeResult result = null!;

        foreach (var scraper in scrapersOrderedByPriority)
        {
            result = await scraper.Scrape(url, httpHeaders);
            if (result.Success)
            {
                logger?.LogInformation("{scraperType} successfully scraped: {Url}", scraper.GetType().Name, url);
                return result;
            }

            logger?.LogInformation("{scraperType} failed scraping: {Url}", scraper.GetType().Name, url);

            Guard.Hope(result.FailureReason != null, "Scrape result FailureReason is null.");
            failureReasons.Add($"{scraper.GetType().Name}: {result.FailureReason}");

            if (result.StopWebScraperChain 
                || result.HttpStatusCode == (int) HttpStatusCode.NotFound)
            {
                break;
            }
        }

        return ScrapeResult.Failed(
            $"Scraping failed:\n{string.Join("\n", failureReasons)}",
            httpStatusCode: result.HttpStatusCode
        );
    }
}