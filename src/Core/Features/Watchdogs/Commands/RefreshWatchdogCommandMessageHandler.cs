using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
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