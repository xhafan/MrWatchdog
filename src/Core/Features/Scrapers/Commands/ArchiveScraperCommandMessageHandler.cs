using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class ArchiveScraperCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<ArchiveScraperCommand>
{
    public async Task Handle(ArchiveScraperCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.Archive();
    }
}