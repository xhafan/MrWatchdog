using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;

public class RefreshWatchdogSearchesDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<ScraperScrapingCompletedDomainEvent>
{
    public async Task Handle(ScraperScrapingCompletedDomainEvent domainEvent)
    {
        var watchdogSearchesIds = await queryExecutor.ExecuteAsync<GetWatchdogSearchesForScraperQuery, long>(
            new GetWatchdogSearchesForScraperQuery(domainEvent.ScraperId)
        );

        foreach (var watchdogSearchId in watchdogSearchesIds)
        {
            await bus.Send(new RefreshWatchdogSearchCommand(watchdogSearchId));
        }
    }
}