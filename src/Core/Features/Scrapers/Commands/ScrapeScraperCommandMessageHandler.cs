using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class ScrapeScraperCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<ScrapeScraperCommand>
{
    public async Task Handle(ScrapeScraperCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);

        await scraper.Scrape(httpClientFactory);
    }
}