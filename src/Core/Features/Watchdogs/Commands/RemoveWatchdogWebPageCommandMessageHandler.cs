using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class RemoveWatchdogWebPageCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<RemoveWatchdogWebPageCommand>
{
    public async Task Handle(RemoveWatchdogWebPageCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.RemoveWebPage(command.WatchdogWebPageId);
    }
}