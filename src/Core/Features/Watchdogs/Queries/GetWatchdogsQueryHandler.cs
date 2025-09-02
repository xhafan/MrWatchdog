using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetWatchdogsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetWatchdogsQuery query)
    {
        GetWatchdogsQueryResult result = null!;
        User user = null!;

        var queryOver = _unitOfWork.Session!.QueryOver<Watchdog>()
            .JoinAlias(x => x.User, () => user)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
            );

        if (query.UserId != 0) // todo: remove this once this query is not used for loading all watchdogs
        {
            queryOver.Where(() => user.Id == query.UserId);
        }

        var results = await queryOver
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetWatchdogsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}