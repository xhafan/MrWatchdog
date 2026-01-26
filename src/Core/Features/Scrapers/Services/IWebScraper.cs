namespace MrWatchdog.Core.Features.Scrapers.Services;

public interface IWebScraper
{
    int Priority { get; }
    bool IsBrowserRenderedHtmlScrapingSupported { get; }

    Task<ScrapeResult> Scrape(
        string url,
        ICollection<(string Name, string Value)>? httpHeaders = null,
        ScrapeOptions? options = null
    );
}