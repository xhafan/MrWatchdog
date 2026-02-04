using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperWebPageScrapedResultsQuery(long ScraperId, long ScraperWebPageId) : IQuery;