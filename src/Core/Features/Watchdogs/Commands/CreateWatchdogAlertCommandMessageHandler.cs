using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogAlertCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IRepository<WatchdogAlert> watchdogAlertRepository,
    NhibernateUnitOfWork unitOfWork
) 
    : IHandleMessages<CreateWatchdogAlertCommand>
{
    public async Task Handle(CreateWatchdogAlertCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);

        var watchdogAlert = await unitOfWork.Session!.QueryOver<WatchdogAlert>()
            .Where(x => x.Watchdog == watchdog 
                        && x.SearchTerm == command.SearchTerm)
            .SingleOrDefaultAsync();
        
        if (watchdogAlert != null) return;

        var newWatchdog = new WatchdogAlert(watchdog, command.SearchTerm);
        await watchdogAlertRepository.SaveAsync(newWatchdog);
    }
}