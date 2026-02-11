using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogScraperArgsQuery(long WatchdogId) : IQuery;