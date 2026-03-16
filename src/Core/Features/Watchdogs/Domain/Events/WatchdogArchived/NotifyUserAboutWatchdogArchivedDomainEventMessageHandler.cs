using CoreBackend.Infrastructure.Rebus;
using CoreDdd.Domain.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;

public class NotifyUserAboutWatchdogArchivedDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    ICoreBus bus
) 
    : IHandleMessages<WatchdogArchivedDomainEvent>
{
    public async Task Handle(WatchdogArchivedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);
        await watchdog.NotifyUserAboutWatchdogArchived(bus);
    }
}