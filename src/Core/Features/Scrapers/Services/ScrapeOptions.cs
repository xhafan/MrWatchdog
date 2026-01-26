using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Services;

public class ScrapeOptions
{
    public ScrapingByBrowserWaitFor? ScrapingByBrowserWaitFor { get; init; }
}