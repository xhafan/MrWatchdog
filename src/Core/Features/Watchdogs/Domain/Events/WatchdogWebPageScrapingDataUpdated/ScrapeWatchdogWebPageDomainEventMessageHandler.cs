using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;

public class ScrapeWatchdogWebPageDomainEventMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<WatchdogWebPageScrapingDataUpdatedDomainEvent>
{
    public async Task Handle(WatchdogWebPageScrapingDataUpdatedDomainEvent domainEvent)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(domainEvent.WatchdogId);

        await watchdog.ScrapeWebPage(domainEvent.WatchdogWebPageId, httpClientFactory);
    }
}