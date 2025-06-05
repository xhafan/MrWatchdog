using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogWebPageArgsQuery(long WatchdogId, long WatchdogWebPageId) : IQuery;