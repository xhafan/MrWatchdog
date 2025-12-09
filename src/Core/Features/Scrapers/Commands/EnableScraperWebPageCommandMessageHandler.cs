using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class EnableScraperWebPageCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<EnableScraperWebPageCommand>
{
    public async Task Handle(EnableScraperWebPageCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.EnableWebPage(command.ScraperWebPageId);
    }
}