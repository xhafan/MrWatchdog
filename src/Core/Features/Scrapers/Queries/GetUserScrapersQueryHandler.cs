using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetUserWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetUserWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetUserWatchdogsQuery query)
    {
        GetUserWatchdogsQueryResult result = null!;
        User user = null!;

        return _unitOfWork.Session!.QueryOver<Watchdog>()
            .JoinAlias(x => x.User, () => user)
            .Where(x => user.Id == query.UserId
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
            )
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetUserWatchdogsQueryResult>());
    }
}