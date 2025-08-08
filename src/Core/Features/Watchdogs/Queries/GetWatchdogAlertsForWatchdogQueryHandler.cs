using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogAlertsForWatchdogQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogAlertsForWatchdogQuery>(unitOfWork)
{
    protected override IQueryOver GetQueryOver<TResult>(GetWatchdogAlertsForWatchdogQuery query)
    {
        Watchdog watchdog = null!;

        return Session.QueryOver<WatchdogAlert>()
            .JoinAlias(x => x.Watchdog, () => watchdog)
            .Where(() => watchdog.Id == query.WatchdogId)
            .Select(x => x.Id);
    }
}