using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogSearchesQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogSearchesQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetWatchdogSearchesQuery query)
    {
        Watchdog watchdog = null!;
        User user = null!;
        GetWatchdogSearchesQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<WatchdogSearch>()
            .JoinAlias(x => x.Watchdog, () => watchdog)
            .JoinAlias(x => x.User, () => user)
            .Where(x => user.Id == query.UserId
                        && !x.IsArchived
                        && !watchdog.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogSearchId)
                .Select(() => watchdog.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.SearchTerm).WithAlias(() => result.SearchTerm)
                .Select(x => x.ReceiveNotification).WithAlias(() => result.ReceiveNotification)
            )
            .OrderBy(() => watchdog.Name).Asc
            .ThenBy(x => x.SearchTerm).Asc
            .TransformUsing(Transformers.AliasToBean<GetWatchdogSearchesQueryResult>());
    }
}