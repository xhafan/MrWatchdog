using CoreDdd.Queries;
using System.Globalization;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetOtherUsersScrapersQuery(long UserId, CultureInfo Culture) : IQuery<GetOtherUsersScrapersQueryResult>;