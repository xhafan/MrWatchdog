using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperWebPageDisabledWarningQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperWebPageDisabledWarningQuery, ScraperWebPageDisabledWarningDto>(unitOfWork)
{
    public override async Task<ScraperWebPageDisabledWarningDto> ExecuteSingleAsync(GetScraperWebPageDisabledWarningQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return scraper.GetScraperWebPageDisabledWarningDto(query.ScraperWebPageId);
    }
}