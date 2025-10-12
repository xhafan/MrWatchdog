using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogSearchArgsQuery(long WatchdogSearchId) : IQuery;