using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogSearchOverviewArgsQuery(long WatchdogSearchId) : IQuery;