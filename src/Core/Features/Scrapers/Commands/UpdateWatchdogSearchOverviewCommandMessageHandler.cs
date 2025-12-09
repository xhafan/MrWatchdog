using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class UpdateWatchdogSearchOverviewCommandMessageHandler(IRepository<WatchdogSearch> watchdogSearchRepository) 
    : IHandleMessages<UpdateWatchdogSearchOverviewCommand>
{
    public async Task Handle(UpdateWatchdogSearchOverviewCommand command)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(command.WatchdogSearchOverviewArgs.WatchdogSearchId);
        watchdogSearch.UpdateOverview(command.WatchdogSearchOverviewArgs);
    }
}