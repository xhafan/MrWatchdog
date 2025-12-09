using CoreDdd.Queries;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetWatchdogSearchesForScraperQuery(long ScraperId) : IQuery;