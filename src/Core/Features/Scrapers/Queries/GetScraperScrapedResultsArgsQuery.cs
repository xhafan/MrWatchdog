using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperScrapedResultsArgsQuery(long ScraperId) : IQuery;