using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events;

public class WatchdogWebPageUpdatedDomainEventMessageHandler(IRepository<Watchdog> watchdogRepository) 
    : IHandleMessages<WatchdogWebPageUpdatedDomainEvent>
{
    public async Task Handle(WatchdogWebPageUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);
    }
}