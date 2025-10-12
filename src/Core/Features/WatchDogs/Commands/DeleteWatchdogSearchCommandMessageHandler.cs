using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class DeleteWatchdogSearchCommandMessageHandler(IRepository<WatchdogSearch> watchdogSearchRepository) 
    : IHandleMessages<DeleteWatchdogSearchCommand>
{
    public async Task Handle(DeleteWatchdogSearchCommand command)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(command.WatchdogSearchId);

        await watchdogSearchRepository.DeleteAsync(watchdogSearch);
    }
}