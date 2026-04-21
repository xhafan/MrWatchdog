using CoreBackend.Features.Account;
using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.Rebus;
using CoreDdd.Domain.Repositories;
using Microsoft.Extensions.Options;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;

public class NotifyUserAboutNewWatchdogScrapedResultsDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    ICoreBus bus,
    IOptions<RuntimeOptions> iRuntimeOptions,
    IOptions<JwtOptions> iJwtOptions
) 
    : IHandleMessages<WatchdogScrapedResultsUpdatedDomainEvent>
{
    public async Task Handle(WatchdogScrapedResultsUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);

        await watchdog.NotifyUserAboutNewScrapedResults(
            bus,
            iRuntimeOptions.Value,
            iJwtOptions.Value
        );
    }
}