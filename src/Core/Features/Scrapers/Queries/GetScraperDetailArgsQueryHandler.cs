using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperDetailArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperDetailArgsQuery, ScraperDetailArgs>(unitOfWork)
{
    public override async Task<ScraperDetailArgs> ExecuteSingleAsync(GetScraperDetailArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return scraper.GetScraperDetailArgs(query.Culture);
    }
}