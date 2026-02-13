using System.Globalization;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
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
        var culture = CultureInfo.GetCultureInfo("en"); // todo: load it from saved user profile later
        await watchdog.NotifyUserAboutWatchdogArchived(culture, bus);
    }
}