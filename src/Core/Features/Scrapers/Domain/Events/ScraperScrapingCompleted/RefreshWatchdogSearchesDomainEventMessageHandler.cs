using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;

public class RefreshWatchdogSearchesDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<WatchdogScrapingCompletedDomainEvent>
{
    public async Task Handle(WatchdogScrapingCompletedDomainEvent domainEvent)
    {
        var watchdogSearchesIds = await queryExecutor.ExecuteAsync<GetWatchdogSearchesForWatchdogQuery, long>(
            new GetWatchdogSearchesForWatchdogQuery(domainEvent.WatchdogId)
        );

        foreach (var watchdogSearchId in watchdogSearchesIds)
        {
            await bus.Send(new RefreshWatchdogSearchCommand(watchdogSearchId));
        }
    }
}