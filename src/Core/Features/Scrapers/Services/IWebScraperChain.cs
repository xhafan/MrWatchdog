namespace MrWatchdog.Core.Features.Scrapers.Services;

public interface IWebScraperChain
{
    Task<ScrapeResult> Scrape(string url, ICollection<(string Name, string Value)>? httpHeaders = null);
}