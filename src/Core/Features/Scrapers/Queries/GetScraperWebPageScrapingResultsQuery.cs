using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogWebPageScrapingResultsQuery(long WatchdogId, long WatchdogWebPageId) : IQuery;