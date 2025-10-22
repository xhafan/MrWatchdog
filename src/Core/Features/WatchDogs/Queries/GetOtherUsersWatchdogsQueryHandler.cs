using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetOtherUsersWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetOtherUsersWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetOtherUsersWatchdogsQuery query)
    {
        User userAlias = null!;
        GetOtherUsersWatchdogsQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<Watchdog>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => userAlias.Id != query.UserId
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
                .Select(() => userAlias.Email).WithAlias(() => result.UserEmail)
            )
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetOtherUsersWatchdogsQueryResult>());
    }
}