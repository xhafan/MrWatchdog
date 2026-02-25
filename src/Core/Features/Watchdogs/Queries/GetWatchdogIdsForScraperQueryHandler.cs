using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogIdsForScraperQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogIdsForScraperQuery, long>(unitOfWork)
{
    protected override IQueryOver GetQueryOver(GetWatchdogIdsForScraperQuery query)
    {
        Scraper scraper = null!;

        return Session.QueryOver<Watchdog>()
            .JoinAlias(x => x.Scraper, () => scraper)
            .Where(x => scraper.Id == query.ScraperId
                        && !x.IsArchived)
            .Select(x => x.Id);
    }
}