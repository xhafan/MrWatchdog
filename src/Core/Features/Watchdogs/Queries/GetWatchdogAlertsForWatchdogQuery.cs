using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogAlertsForWatchdogQuery(long WatchdogId) : IQuery;