using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class MakeWatchdogPublicCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<MakeWatchdogPublicCommand>
{
    public async Task Handle(MakeWatchdogPublicCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.MakePublic();
    }
}