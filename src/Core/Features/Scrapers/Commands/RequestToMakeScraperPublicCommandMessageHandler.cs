using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Scrapers.Domain;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class RequestToMakeScraperPublicCommandMessageHandler(IRepository<Scraper> scraperRepository) 
    : IHandleMessages<RequestToMakeScraperPublicCommand>
{
    public async Task Handle(RequestToMakeScraperPublicCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        scraper.RequestToMakePublic();
    }
}