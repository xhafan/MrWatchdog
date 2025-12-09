using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetPublicScraperStatisticsQuery(long ScraperId) : IQuery;