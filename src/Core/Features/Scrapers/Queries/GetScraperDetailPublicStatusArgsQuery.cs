using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperDetailPublicStatusArgsQuery(long ScraperId) : IQuery<ScraperDetailPublicStatusArgs>;