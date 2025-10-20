using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetPublicWatchdogStatisticsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetPublicWatchdogStatisticsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetPublicWatchdogStatisticsQuery query)
    {
        Watchdog watchdogAlias = null!;
        GetPublicWatchdogStatisticsQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<WatchdogSearch>()
            .JoinAlias(x => x.Watchdog, () => watchdogAlias)
            .Where(x => x.Watchdog.Id == query.WatchdogId
                        && x.User.Id != watchdogAlias.User.Id)
            .SelectList(list => list
                .SelectGroup(x => x.ReceiveNotification).WithAlias(() => result.ReceiveNotification)
                .SelectCountDistinct(x => x.User.Id).WithAlias(() => result.CountOfWatchdogSearches)
            )
            .TransformUsing(Transformers.AliasToBean<GetPublicWatchdogStatisticsQueryResult>());
    }
}