using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetWatchdogSearchArgsQuery(long WatchdogSearchId) : IQuery;