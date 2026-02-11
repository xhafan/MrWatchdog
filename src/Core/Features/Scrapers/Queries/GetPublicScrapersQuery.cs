using System.Globalization;
using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetPublicScrapersQuery(CultureInfo Culture) : IQuery;