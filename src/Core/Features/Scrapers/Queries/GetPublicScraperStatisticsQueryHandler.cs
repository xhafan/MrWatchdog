using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetPublicScraperStatisticsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetPublicScraperStatisticsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetPublicScraperStatisticsQuery query)
    {
        Scraper scraperAlias = null!;
        GetPublicScraperStatisticsQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<Watchdog>()
            .JoinAlias(x => x.Scraper, () => scraperAlias)
            .Where(x => x.Scraper.Id == query.ScraperId
                        && x.User.Id != scraperAlias.User.Id)
            .SelectList(list => list
                .SelectGroup(x => x.ReceiveNotification).WithAlias(() => result.ReceiveNotification)
                .SelectCountDistinct(x => x.User.Id).WithAlias(() => result.CountOfWatchdogs)
            )
            .TransformUsing(Transformers.AliasToBean<GetPublicScraperStatisticsQueryResult>());
    }
}