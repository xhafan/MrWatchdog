using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class UpdateWatchdogOverviewCommandMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<UpdateWatchdogOverviewCommand>
{
    public async Task Handle(UpdateWatchdogOverviewCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogOverviewArgs.Id);
        watchdog.UpdateOverview(command.WatchdogOverviewArgs);
    }
}