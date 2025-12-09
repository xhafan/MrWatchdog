using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class UpdateScraperWebPageCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<UpdateScraperWebPageCommand>
{
    public async Task Handle(UpdateScraperWebPageCommand command)
    {
        var async = await scraperRepository.LoadByIdAsync(command.ScraperWebPageArgs.ScraperId);
        async.UpdateWebPage(command.ScraperWebPageArgs);
    }
}