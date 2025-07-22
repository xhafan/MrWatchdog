using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogScrapingResultsArgsQuery(long WatchdogId) : IQuery;