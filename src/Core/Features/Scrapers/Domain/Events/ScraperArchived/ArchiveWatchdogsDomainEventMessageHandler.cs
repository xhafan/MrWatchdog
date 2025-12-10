using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperArchived;

public class ArchiveWatchdogsDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<ScraperArchivedDomainEvent>
{
    public async Task Handle(ScraperArchivedDomainEvent domainEvent)
    {
        var watchdogIds = await queryExecutor.ExecuteAsync<GetWatchdogsForScraperQuery, long>(
            new GetWatchdogsForScraperQuery(domainEvent.ScraperId)
        );

        foreach (var watchdogId in watchdogIds)
        {
            await bus.Send(new ArchiveWatchdogCommand(watchdogId));
        }
    }
}