using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class UpdateWatchdogWebPageCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<UpdateWatchdogWebPageCommand>
{
    public async Task Handle(UpdateWatchdogWebPageCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogWebPageArgs.WatchdogId);
        watchdog.UpdateWebPage(command.WatchdogWebPageArgs);
    }
}