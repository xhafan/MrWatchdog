using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate.Transform;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogAlertsQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseNhibernateQueryHandler<GetWatchdogAlertsQuery>(unitOfWork)
{
    private readonly NhibernateUnitOfWork _unitOfWork = unitOfWork;

    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetWatchdogAlertsQuery query)
    {
        Watchdog watchdog = null!;
        GetWatchdogAlertsQueryResult result = null!;

        var results = await _unitOfWork.Session!.QueryOver<WatchdogAlert>()
            .JoinAlias(x => x.Watchdog, () => watchdog)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogAlertId)
                .Select(() => watchdog.Name).WithAlias(() => result.WatchdogName)
                .Select(x => x.SearchTerm).WithAlias(() => result.SearchTerm)
            )
            .OrderByAlias(() => result.WatchdogAlertId).Desc
            .TransformUsing(Transformers.AliasToBean<GetWatchdogAlertsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}