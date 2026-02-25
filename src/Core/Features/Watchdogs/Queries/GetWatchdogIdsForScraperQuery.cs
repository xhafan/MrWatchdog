using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public record GetWatchdogIdsForScraperQuery(long ScraperId) : IQuery<long>;