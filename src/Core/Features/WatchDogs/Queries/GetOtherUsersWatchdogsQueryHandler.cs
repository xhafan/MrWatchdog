using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetOtherUsersWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetOtherUsersWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetOtherUsersWatchdogsQuery query)
    {
        Watchdog watchdogAlias = null!;
        GetOtherUsersWatchdogsQueryResult result = null!;
        User user = null!;

        var queryOver = _unitOfWork.Session!.QueryOver(() => watchdogAlias)
            .JoinAlias(x => x.User, () => user)
            .Where(() => user.Id != query.UserId)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.PublicStatus).WithAlias(() => result.PublicStatus)
                .Select(() => user.Id).WithAlias(() => result.UserId)
                .Select(() => user.Email).WithAlias(() => result.UserEmail)
            );
        
        var results = await queryOver
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetOtherUsersWatchdogsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}