using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogSearchCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IRepository<WatchdogSearch> watchdogSearchRepository,
    IUserRepository userRepository,
    NhibernateUnitOfWork unitOfWork
) 
    : IHandleMessages<CreateWatchdogSearchCommand>
{
    public async Task Handle(CreateWatchdogSearchCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        var user = await userRepository.LoadByIdAsync(command.ActingUserId);

        var watchdogSearches = await unitOfWork.Session!.QueryOver<WatchdogSearch>()
            .Where(x => x.Watchdog == watchdog 
                        && x.User == user
                        && x.SearchTerm == command.SearchTerm
                        && !x.IsArchived
            )
            .ListAsync();
        
        if (watchdogSearches.Any()) return;

        var newWatchdog = new WatchdogSearch(watchdog, user, command.SearchTerm);
        await watchdogSearchRepository.SaveAsync(newWatchdog);
    }
}