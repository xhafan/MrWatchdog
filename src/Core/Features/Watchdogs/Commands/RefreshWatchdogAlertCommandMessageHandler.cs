using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class RefreshWatchdogAlertCommandMessageHandler(
    IRepository<WatchdogAlert> watchdogAlertRepository
) 
    : IHandleMessages<RefreshWatchdogAlertCommand>
{
    public async Task Handle(RefreshWatchdogAlertCommand command)
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(command.WatchdogAlertId);

        watchdogAlert.Refresh();
    }
}