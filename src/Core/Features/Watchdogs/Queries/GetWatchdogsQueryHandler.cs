using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
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
        GetWatchdogsQueryResult getWatchdogsQueryResult = null!;

        var results = await _unitOfWork.Session!.QueryOver<Watchdog>()
            .SelectList(list => list
                .Select(x => x.Id).WithAlias(() => getWatchdogsQueryResult.Id)
                .Select(x => x.Name).WithAlias(() => getWatchdogsQueryResult.Name)
            )
            .TransformUsing(Transformers.AliasToBean<GetWatchdogsQueryResult>())
            .ListAsync<TResult>();
        
        return results;
    }
}