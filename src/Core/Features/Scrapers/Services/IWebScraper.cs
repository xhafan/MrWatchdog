namespace MrWatchdog.Core.Features.Scrapers.Services;

public interface IWebScraper
{
    int Priority { get; }
    Task<ScrapeResult> Scrape(string url, ICollection<(string Name, string Value)>? httpHeaders = null);
}