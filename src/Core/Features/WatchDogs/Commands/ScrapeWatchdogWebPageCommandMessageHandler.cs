using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class ScrapeWatchdogWebPageCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<ScrapeWatchdogWebPageCommand>
{
    public async Task Handle(ScrapeWatchdogWebPageCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);

        await watchdog.ScrapeWebPage(
            command.WatchdogWebPageId,
            httpClientFactory,
            canRaiseScrapingFailedDomainEvent: false
        );
    }
}