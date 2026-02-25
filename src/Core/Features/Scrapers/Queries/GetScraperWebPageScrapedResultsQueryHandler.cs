using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperWebPageScrapedResultsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperWebPageScrapedResultsQuery, ScraperWebPageScrapedResultsDto>(unitOfWork)
{
    public override async Task<ScraperWebPageScrapedResultsDto> ExecuteSingleAsync(GetScraperWebPageScrapedResultsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return scraper.GetScraperWebPageScrapedResultsDto(query.ScraperWebPageId);
    }
}