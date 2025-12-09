using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetWatchdogSearchesQuery(long UserId) : IQuery;