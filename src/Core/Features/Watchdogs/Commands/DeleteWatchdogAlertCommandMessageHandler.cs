using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class DeleteWatchdogAlertCommandMessageHandler(IRepository<WatchdogAlert> watchdogAlertRepository) 
    : IHandleMessages<DeleteWatchdogAlertCommand>
{
    public async Task Handle(DeleteWatchdogAlertCommand command)
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(command.WatchdogAlertId);

        await watchdogAlertRepository.DeleteAsync(watchdogAlert);
    }
}