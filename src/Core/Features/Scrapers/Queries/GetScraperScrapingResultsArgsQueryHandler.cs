using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperScrapingResultsArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperScrapingResultsArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetScraperScrapingResultsArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return (TResult)(object)scraper.GetScraperScrapingResultsArgs();
    }
}