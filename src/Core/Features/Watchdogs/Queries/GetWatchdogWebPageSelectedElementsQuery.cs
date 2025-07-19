using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogWebPageSelectedElementsQuery(long WatchdogId, long WatchdogWebPageId) : IQuery;