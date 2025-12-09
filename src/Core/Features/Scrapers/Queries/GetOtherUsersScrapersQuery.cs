using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetOtherUsersScrapersQuery(long UserId) : IQuery;