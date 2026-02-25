using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Scrapers.Domain;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class UpdateScraperOverviewCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<UpdateScraperOverviewCommand>
{
    public async Task Handle(UpdateScraperOverviewCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperOverviewArgs.ScraperId);
        scraper.UpdateOverview(command.ScraperOverviewArgs);
    }
}