using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperDetailPublicStatusArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperDetailPublicStatusArgsQuery, ScraperDetailPublicStatusArgs>(unitOfWork)
{
    public override async Task<ScraperDetailPublicStatusArgs> ExecuteSingleAsync(GetScraperDetailPublicStatusArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return scraper.GetScraperDetailPublicStatusArgs();
    }
}