using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogSearchesForWatchdogQuery(long WatchdogId) : IQuery;