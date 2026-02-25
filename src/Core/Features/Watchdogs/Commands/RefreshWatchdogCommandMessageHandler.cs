using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class RefreshWatchdogCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository
) 
    : IHandleMessages<RefreshWatchdogCommand>
{
    public async Task Handle(RefreshWatchdogCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);

        watchdog.Refresh();
    }
}