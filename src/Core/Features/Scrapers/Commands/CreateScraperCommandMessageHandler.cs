using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class CreateScraperCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IUserRepository userRepository
) 
    : IHandleMessages<CreateScraperCommand>
{
    public async Task Handle(CreateScraperCommand command)
    {
        var user = await userRepository.LoadByIdAsync(command.UserId);
        var newScraper = new Scraper(user, command.Name);
        await scraperRepository.SaveAsync(newScraper);
    }
}