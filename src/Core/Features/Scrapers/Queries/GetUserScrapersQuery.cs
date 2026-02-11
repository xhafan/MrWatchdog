using System.Globalization;
using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetUserScrapersQuery(long UserId, CultureInfo Culture) : IQuery;