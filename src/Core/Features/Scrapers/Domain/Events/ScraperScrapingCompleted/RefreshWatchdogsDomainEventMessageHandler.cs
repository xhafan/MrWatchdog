using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;

public class RefreshWatchdogsDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<ScraperScrapingCompletedDomainEvent>
{
    public async Task Handle(ScraperScrapingCompletedDomainEvent domainEvent)
    {
        var watchdogIds = await queryExecutor.ExecuteAsync<GetWatchdogsForScraperQuery, long>(
            new GetWatchdogsForScraperQuery(domainEvent.ScraperId)
        );

        foreach (var watchdogId in watchdogIds)
        {
            await bus.Send(new RefreshWatchdogCommand(watchdogId));
        }
    }
}