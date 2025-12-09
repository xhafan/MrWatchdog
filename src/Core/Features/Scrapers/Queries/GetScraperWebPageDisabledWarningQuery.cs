using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperWebPageDisabledWarningQuery(long ScraperId, long ScraperWebPageId) : IQuery;