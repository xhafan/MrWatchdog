using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperDetailPublicStatusArgsQuery(long ScraperId) : IQuery;