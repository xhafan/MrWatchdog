using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingResultsUpdated;

public class NotifyUserAboutNewWatchdogScrapingResultsDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<WatchdogScrapingResultsUpdatedDomainEvent>
{
    public async Task Handle(WatchdogScrapingResultsUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);

        await watchdog.NotifyUserAboutNewScrapingResults(bus, iRuntimeOptions.Value);
    }
}