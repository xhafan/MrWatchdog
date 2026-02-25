using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperOverviewArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperOverviewArgsQuery, ScraperOverviewArgs>(unitOfWork)
{
    public override async Task<ScraperOverviewArgs> ExecuteSingleAsync(GetScraperOverviewArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return scraper.GetScraperOverviewArgs();
    }
}