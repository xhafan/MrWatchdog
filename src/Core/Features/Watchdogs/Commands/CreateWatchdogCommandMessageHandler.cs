using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Commands;

public class CreateWatchdogSearchCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IRepository<WatchdogSearch> watchdogSearchRepository,
    IUserRepository userRepository,
    NhibernateUnitOfWork unitOfWork
) 
    : IHandleMessages<CreateWatchdogSearchCommand>
{
    public async Task Handle(CreateWatchdogSearchCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        var user = await userRepository.LoadByIdAsync(command.ActingUserId);

        var watchdogSearches = await unitOfWork.Session!.QueryOver<WatchdogSearch>()
            .Where(x => x.Scraper == scraper 
                        && x.User == user
                        && x.SearchTerm == command.SearchTerm
                        && !x.IsArchived
            )
            .ListAsync();
        
        if (watchdogSearches.Any()) return;

        var newWatchdogSearch = new WatchdogSearch(scraper, user, command.SearchTerm);
        await watchdogSearchRepository.SaveAsync(newWatchdogSearch);
    }
}