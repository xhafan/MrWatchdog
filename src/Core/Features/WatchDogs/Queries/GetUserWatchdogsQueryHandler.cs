using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetUserWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetUserWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetUserWatchdogsQuery query)
    {
        Watchdog watchdogAlias = null!;
        GetUserWatchdogsQueryResult result = null!;
        User user = null!;

        var queryOver = _unitOfWork.Session!.QueryOver(() => watchdogAlias)
            .JoinAlias(x => x.User, () => user)
            .Where(() => user.Id == query.UserId)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
            );

        var results = await queryOver
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetUserWatchdogsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}