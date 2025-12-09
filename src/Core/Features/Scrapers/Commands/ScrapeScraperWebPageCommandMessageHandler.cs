using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class ScrapeScraperWebPageCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<ScrapeScraperWebPageCommand>
{
    public async Task Handle(ScrapeScraperWebPageCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);

        await scraper.ScrapeWebPage(
            command.ScraperWebPageId,
            httpClientFactory,
            canRaiseScrapingFailedDomainEvent: false
        );
    }
}