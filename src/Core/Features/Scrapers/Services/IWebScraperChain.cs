namespace MrWatchdog.Core.Features.Scrapers.Services;

public interface IWebScraperChain
{
    Task<ScrapeResult> Scrape(
        string url,
        bool scrapeHtmlAsRenderedByBrowser,
        ICollection<(string Name, string Value)>? httpHeaders = null,
        ScrapeOptions? options = null
    );
}