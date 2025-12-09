using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetWatchdogSearchOverviewArgsQuery(long WatchdogSearchId) : IQuery;