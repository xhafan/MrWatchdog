using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperScrapingResultsArgsQuery(long ScraperId) : IQuery;