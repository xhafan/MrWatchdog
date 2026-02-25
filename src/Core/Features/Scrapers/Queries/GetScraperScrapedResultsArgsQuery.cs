using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Domain;
using System.Globalization;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperScrapedResultsArgsQuery(
    long ScraperId,
    CultureInfo Culture
    ) : IQuery<ScraperScrapedResultsArgs>;