using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class RequestToMakeWatchdogPublicCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<RequestToMakeWatchdogPublicCommand>
{
    public async Task Handle(RequestToMakeWatchdogPublicCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);
        watchdog.RequestToMakePublic();
    }
}