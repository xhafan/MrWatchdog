using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetPublicWatchdogStatisticsQuery(long WatchdogId) : IQuery;