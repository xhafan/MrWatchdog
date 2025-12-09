using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class MakeScraperPrivateCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<MakeScraperPrivateCommand>
{
    public async Task Handle(MakeScraperPrivateCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.MakePrivate();
    }
}