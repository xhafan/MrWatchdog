using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;

public class ScrapeScraperWebPageDomainEventMessageHandler(
    IRepository<Scraper> scraperRepository,
    IWebScraperChain webScraperChain
) 
    : IHandleMessages<ScraperWebPageScrapingDataUpdatedDomainEvent>
{
    public async Task Handle(ScraperWebPageScrapingDataUpdatedDomainEvent domainEvent)
    {
        var scraper = await scraperRepository.LoadByIdAsync(domainEvent.ScraperId);

        await scraper.ScrapeWebPage(
            domainEvent.ScraperWebPageId,
            webScraperChain,
            canRaiseScrapingFailedDomainEvent: false
        );
    }
}