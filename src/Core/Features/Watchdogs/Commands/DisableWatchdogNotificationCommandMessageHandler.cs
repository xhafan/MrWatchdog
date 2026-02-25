using CoreDdd.Domain.Repositories;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class DisableWatchdogNotificationCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository
) 
    : IHandleMessages<DisableWatchdogNotificationCommand>
{
    public async Task Handle(DisableWatchdogNotificationCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);

        watchdog.DisableNotification();
    }
}