using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class ScrapeWatchdogCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IHttpClientFactory httpClientFactory
) 
    : IHandleMessages<ScrapeWatchdogCommand>
{
    public async Task Handle(ScrapeWatchdogCommand command)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(command.WatchdogId);

        await watchdog.Scrape(httpClientFactory);
    }
}