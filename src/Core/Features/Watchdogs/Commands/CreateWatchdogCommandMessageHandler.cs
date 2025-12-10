using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogCommandMessageHandler(
    IRepository<Scraper> scraperRepository,
    IRepository<Watchdog> watchdogRepository,
    IUserRepository userRepository,
    NhibernateUnitOfWork unitOfWork
) 
    : IHandleMessages<CreateWatchdogCommand>
{
    public async Task Handle(CreateWatchdogCommand command)
    {
        var scraper = await scraperRepository.LoadByIdAsync(command.ScraperId);
        var user = await userRepository.LoadByIdAsync(command.ActingUserId);

        var watchdogs = await unitOfWork.Session!.QueryOver<Watchdog>()
            .Where(x => x.Scraper == scraper 
                        && x.User == user
                        && x.SearchTerm == command.SearchTerm
                        && !x.IsArchived
            )
            .ListAsync();
        
        if (watchdogs.Any()) return;

        var newWatchdog = new Watchdog(scraper, user, command.SearchTerm);
        await watchdogRepository.SaveAsync(newWatchdog);
    }
}