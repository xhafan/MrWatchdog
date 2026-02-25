using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Scrapers.Domain;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class MakeScraperPublicCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<MakeScraperPublicCommand>
{
    public async Task Handle(MakeScraperPublicCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.MakePublic();
    }
}