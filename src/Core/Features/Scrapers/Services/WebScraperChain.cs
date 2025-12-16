using CoreUtils;
using Microsoft.Extensions.Logging;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class WebScraperChain(
    IEnumerable<IWebScraper> scrapers,
    ILogger<WebScraperChain>? logger = null
) : IWebScraperChain
{
    public async Task<ScrapeResult> Scrape(string url, ICollection<(string Name, string Value)>? httpHeaders)
    {
        var scrapersOrderedByPriority = scrapers
            .OrderBy(s => s.Priority)
            .ToList();

        var failureReasons = new List<string>();

        foreach (var scraper in scrapersOrderedByPriority)
        {
            var result = await scraper.Scrape(url, httpHeaders);
            if (result.Success)
            {
                logger?.LogInformation("{scraperType} successfully scraped: {Url}", scraper.GetType().Name, url);
                return result;
            }

            logger?.LogInformation("{scraperType} failed scraping: {Url}", scraper.GetType().Name, url);

            Guard.Hope(result.FailureReason != null, "Scrape result FailureReason is null.");
            failureReasons.Add($"{scraper.GetType().Name}: {result.FailureReason}");

            if (result.StopWebScraperChain) break;
        }

        return ScrapeResult.Failed($"Scraping failed:{Environment.NewLine}{string.Join(Environment.NewLine, failureReasons)}");
    }
}