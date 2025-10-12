using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogSearchesQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetWatchdogSearchesQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetWatchdogSearchesQuery query)
    {
        Watchdog watchdog = null!;
        User user = null!;
        GetWatchdogSearchesQueryResult result = null!;

        var results = await _unitOfWork.Session!.QueryOver<WatchdogSearch>()
            .JoinAlias(x => x.Watchdog, () => watchdog)
            .JoinAlias(x => x.User, () => user)
            .Where(() => user.Id == query.UserId)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogSearchId)
                .Select(() => watchdog.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.SearchTerm).WithAlias(() => result.SearchTerm)
            )
            .OrderBy(() => watchdog.Name).Asc
            .ThenBy(x => x.SearchTerm).Asc
            .TransformUsing(Transformers.AliasToBean<GetWatchdogSearchesQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}