using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;

public class ArchiveWatchdogSearchesDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<WatchdogArchivedDomainEvent>
{
    public async Task Handle(WatchdogArchivedDomainEvent domainEvent)
    {
        var watchdogSearchesIds = await queryExecutor.ExecuteAsync<GetWatchdogSearchesForWatchdogQuery, long>(
            new GetWatchdogSearchesForWatchdogQuery(domainEvent.WatchdogId)
        );

        foreach (var watchdogSearchId in watchdogSearchesIds)
        {
            await bus.Send(new ArchiveWatchdogSearchCommand(watchdogSearchId));
        }
    }
}