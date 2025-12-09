using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogOverviewArgsQuery(long WatchdogId) : IQuery;