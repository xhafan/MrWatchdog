using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class RefreshWatchdogSearchCommandMessageHandler(
    IRepository<WatchdogSearch> watchdogSearchRepository
) 
    : IHandleMessages<RefreshWatchdogSearchCommand>
{
    public async Task Handle(RefreshWatchdogSearchCommand command)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(command.WatchdogSearchId);

        watchdogSearch.Refresh();
    }
}