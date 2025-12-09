using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;

public class ScrapeScraperWebPageDomainEventMessageHandler(
    IRepository<Scraper> scraperRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<ScraperWebPageScrapingDataUpdatedDomainEvent>
{
    public async Task Handle(ScraperWebPageScrapingDataUpdatedDomainEvent domainEvent)
    {
        var scraper = await scraperRepository.LoadByIdAsync(domainEvent.ScraperId);

        await scraper.ScrapeWebPage(
            domainEvent.ScraperWebPageId,
            httpClientFactory,
            canRaiseScrapingFailedDomainEvent: false
        );
    }
}