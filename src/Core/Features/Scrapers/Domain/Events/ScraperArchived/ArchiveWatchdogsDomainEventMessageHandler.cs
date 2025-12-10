using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperArchived;

public class ArchiveWatchdogSearchesDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<ScraperArchivedDomainEvent>
{
    public async Task Handle(ScraperArchivedDomainEvent domainEvent)
    {
        var watchdogSearchesIds = await queryExecutor.ExecuteAsync<GetWatchdogSearchesForScraperQuery, long>(
            new GetWatchdogSearchesForScraperQuery(domainEvent.ScraperId)
        );

        foreach (var watchdogSearchId in watchdogSearchesIds)
        {
            await bus.Send(new ArchiveWatchdogSearchCommand(watchdogSearchId));
        }
    }
}