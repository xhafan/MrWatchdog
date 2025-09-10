using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogDetailPublicStatusArgsQuery(long WatchdogId) : IQuery;