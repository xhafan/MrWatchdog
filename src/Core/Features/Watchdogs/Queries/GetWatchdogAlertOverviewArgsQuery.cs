using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogAlertOverviewArgsQuery(long WatchdogAlertId) : IQuery;