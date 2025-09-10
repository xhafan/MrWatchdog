using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class MakeWatchdogPrivateCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<MakeWatchdogPrivateCommand>
{
    public async Task Handle(MakeWatchdogPrivateCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.MakePrivate();
    }
}