using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetScraperScrapedResultsArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Scraper> scraperRepository
) : BaseNhibernateQueryHandler<GetScraperScrapedResultsArgsQuery, ScraperScrapedResultsArgs>(unitOfWork)
{
    public override async Task<ScraperScrapedResultsArgs> ExecuteSingleAsync(GetScraperScrapedResultsArgsQuery query)
    {
        var scraper = await scraperRepository.LoadByIdAsync(query.ScraperId);
        return scraper.GetScraperScrapedResultsArgs(query.Culture);
    }
}