using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperWebPageScrapedResultsQuery(long ScraperId, long ScraperWebPageId) : IQuery<ScraperWebPageScrapedResultsDto>;