using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class ScrapeScraperCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IWebScraperChain webScraperChain
) 
    : IHandleMessages<ScrapeScraperCommand>
{
    public async Task Handle(ScrapeScraperCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);

        await scraper.Scrape(webScraperChain);
    }
}