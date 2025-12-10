using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogsForScraperQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogsForScraperQuery>(unitOfWork)
{
    protected override IQueryOver GetQueryOver<TResult>(GetWatchdogsForScraperQuery query)
    {
        Scraper scraper = null!;

        return Session.QueryOver<Watchdog>()
            .JoinAlias(x => x.Scraper, () => scraper)
            .Where(x => scraper.Id == query.ScraperId
                        && !x.IsArchived)
            .Select(x => x.Id);
    }
}