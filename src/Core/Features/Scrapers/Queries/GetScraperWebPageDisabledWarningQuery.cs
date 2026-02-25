using CoreDdd.Queries;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public record GetScraperWebPageDisabledWarningQuery(long ScraperId, long ScraperWebPageId) : IQuery<ScraperWebPageDisabledWarningDto>;