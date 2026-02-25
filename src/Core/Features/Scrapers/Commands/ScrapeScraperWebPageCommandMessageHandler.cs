using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Services;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class ScrapeScraperWebPageCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IWebScraperChain webScraperChain
) 
    : IHandleMessages<ScrapeScraperWebPageCommand>
{
    public async Task Handle(ScrapeScraperWebPageCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);

        await scraper.ScrapeWebPage(
            command.ScraperWebPageId,
            webScraperChain,
            canRaiseScrapingFailedDomainEvent: false
        );
    }
}