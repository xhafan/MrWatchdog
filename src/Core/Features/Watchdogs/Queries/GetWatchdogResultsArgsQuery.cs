using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogResultsArgsQuery(long WatchdogId) : IQuery;