using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogAlertArgsQuery(long WatchdogAlertId) : IQuery;