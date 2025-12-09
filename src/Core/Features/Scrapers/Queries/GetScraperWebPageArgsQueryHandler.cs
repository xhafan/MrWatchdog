using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperWebPageArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperWebPageArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetScraperWebPageArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return (TResult)(object)scraper.GetScraperWebPageArgs(query.ScraperWebPageId);
    }
}