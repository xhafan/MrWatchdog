using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Scrapers.Domain;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class RemoveScraperWebPageCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<RemoveScraperWebPageCommand>
{
    public async Task Handle(RemoveScraperWebPageCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.RemoveWebPage(command.ScraperWebPageId);
    }
}