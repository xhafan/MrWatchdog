using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperWebPageScrapingResultsQuery(long ScraperId, long ScraperWebPageId) : IQuery;