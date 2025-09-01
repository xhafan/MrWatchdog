using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;

public class RefreshWatchdogAlertsDomainEventMessageHandler(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) 
    : IHandleMessages<WatchdogScrapingCompletedDomainEvent>
{
    public async Task Handle(WatchdogScrapingCompletedDomainEvent domainEvent)
    {
        var watchdogAlertsIds = await queryExecutor.ExecuteAsync<GetWatchdogAlertsForWatchdogQuery, long>(
            new GetWatchdogAlertsForWatchdogQuery(domainEvent.WatchdogId)
        );

        foreach (var watchdogAlertId in watchdogAlertsIds)
        {
            await bus.Send(new RefreshWatchdogAlertCommand(watchdogAlertId));
        }
    }
}