using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class ArchiveWatchdogCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<ArchiveWatchdogCommand>
{
    public async Task Handle(ArchiveWatchdogCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.Archive();
    }
}