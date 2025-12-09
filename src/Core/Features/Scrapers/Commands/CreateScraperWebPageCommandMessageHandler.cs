using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class CreateScraperWebPageCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<CreateScraperWebPageCommand>
{
    public async Task Handle(CreateScraperWebPageCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.AddWebPage(new ScraperWebPageArgs());
    }
}