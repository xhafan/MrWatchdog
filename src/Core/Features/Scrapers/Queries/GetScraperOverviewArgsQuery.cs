using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperOverviewArgsQuery(long ScraperId) : IQuery<ScraperOverviewArgs>;