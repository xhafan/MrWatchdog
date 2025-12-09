using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using NHibernate;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetWatchdogSearchesForScraperQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogSearchesForScraperQuery>(unitOfWork)
{
    protected override IQueryOver GetQueryOver<TResult>(GetWatchdogSearchesForScraperQuery query)
    {
        Scraper scraper = null!;

        return Session.QueryOver<WatchdogSearch>()
            .JoinAlias(x => x.Scraper, () => scraper)
            .Where(x => scraper.Id == query.ScraperId
                        && !x.IsArchived)
            .Select(x => x.Id);
    }
}