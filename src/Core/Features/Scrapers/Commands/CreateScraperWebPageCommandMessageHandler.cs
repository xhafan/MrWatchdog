using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogWebPageCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<CreateWatchdogWebPageCommand>
{
    public async Task Handle(CreateWatchdogWebPageCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.AddWebPage(new WatchdogWebPageArgs());
    }
}