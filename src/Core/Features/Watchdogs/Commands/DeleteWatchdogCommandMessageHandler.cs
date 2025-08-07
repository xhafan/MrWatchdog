using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class DeleteWatchdogCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<DeleteWatchdogCommand>
{
    public async Task Handle(DeleteWatchdogCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);

        await watchdogRepository.DeleteAsync(watchdog);
    }
}