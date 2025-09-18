using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
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
        Watchdog watchdogAlias = null!;
        GetPublicWatchdogsQueryResult result = null!;

        var queryOver = _unitOfWork.Session!.QueryOver(() => watchdogAlias)
            .Where(() => watchdogAlias.PublicStatus == PublicStatus.Public)
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => result.WatchdogId)
                .Select(x => x.Name).WithAlias(() => result.WatchdogName)
            );

        var results = await queryOver
            .OrderByAlias(() => result.WatchdogName).Asc
            .TransformUsing(Transformers.AliasToBean<GetPublicWatchdogsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}