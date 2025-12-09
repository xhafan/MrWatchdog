using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperWebPageArgsQuery(long ScraperId, long ScraperWebPageId) : IQuery;