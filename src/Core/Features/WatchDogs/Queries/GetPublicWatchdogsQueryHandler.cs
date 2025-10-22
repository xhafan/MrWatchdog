using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetPublicWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetPublicWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    protected override IQueryOver GetQueryOver<TResult>(GetPublicWatchdogsQuery query)
    {
        User userAlias = null!;
        GetPublicWatchdogsQueryResult result = null!;

        return _unitOfWork.Session!.QueryOver<Watchdog>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => x.PublicStatus == PublicStatus.Public
                        && !x.IsArchived)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
            )
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetPublicWatchdogsQueryResult>());
    }
}