using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class EnableWatchdogWebPageCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<EnableWatchdogWebPageCommand>
{
    public async Task Handle(EnableWatchdogWebPageCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.EnableWebPage(command.WatchdogWebPageId);
    }
}