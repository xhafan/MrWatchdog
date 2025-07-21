using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogDetailArgsQuery(long WatchdogId) : IQuery;