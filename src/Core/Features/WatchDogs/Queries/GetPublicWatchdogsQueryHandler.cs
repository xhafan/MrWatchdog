using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetPublicWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetPublicWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetPublicWatchdogsQuery query)
    {
        User userAlias = null!;
        GetPublicWatchdogsQueryResult result = null!;

        var queryOver = _unitOfWork.Session!.QueryOver<Watchdog>()
            .JoinAlias(x => x.User, () => userAlias)
            .Where(x => x.PublicStatus == PublicStatus.Public)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
                .Select(() => userAlias.Id).WithAlias(() => result.UserId)
            );

        var results = await queryOver
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetPublicWatchdogsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}