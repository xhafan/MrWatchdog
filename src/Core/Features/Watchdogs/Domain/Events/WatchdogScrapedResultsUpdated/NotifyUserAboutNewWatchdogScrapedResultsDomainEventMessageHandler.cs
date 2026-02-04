using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;

public class NotifyUserAboutNewWatchdogScrapedResultsDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<WatchdogScrapedResultsUpdatedDomainEvent>
{
    public async Task Handle(WatchdogScrapedResultsUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);

        await watchdog.NotifyUserAboutNewScrapedResults(bus, iRuntimeOptions.Value);
    }
}