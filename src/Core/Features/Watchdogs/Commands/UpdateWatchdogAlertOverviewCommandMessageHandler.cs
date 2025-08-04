using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class UpdateWatchdogAlertOverviewCommandMessageHandler(IRepository<WatchdogAlert> watchdogAlertRepository) 
    : IHandleMessages<UpdateWatchdogAlertOverviewCommand>
{
    public async Task Handle(UpdateWatchdogAlertOverviewCommand command)
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(command.WatchdogAlertOverviewArgs.WatchdogAlertId);
        watchdogAlert.UpdateOverview(command.WatchdogAlertOverviewArgs);
    }
}