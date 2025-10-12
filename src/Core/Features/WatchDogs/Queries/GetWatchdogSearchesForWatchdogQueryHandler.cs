using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using NHibernate;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogSearchesForWatchdogQueryHandler(
    NhibernateUnitOfWork unitOfWork
) : BaseQueryOverHandler<GetWatchdogSearchesForWatchdogQuery>(unitOfWork)
{
    protected override IQueryOver GetQueryOver<TResult>(GetWatchdogSearchesForWatchdogQuery query)
    {
        Watchdog watchdog = null!;

        return Session.QueryOver<WatchdogSearch>()
            .JoinAlias(x => x.Watchdog, () => watchdog)
            .Where(() => watchdog.Id == query.WatchdogId)
            .Select(x => x.Id);
    }
}