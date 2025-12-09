using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperDetailPublicStatusArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperDetailPublicStatusArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetScraperDetailPublicStatusArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return (TResult)(object)scraper.GetScraperDetailPublicStatusArgs();
    }
}