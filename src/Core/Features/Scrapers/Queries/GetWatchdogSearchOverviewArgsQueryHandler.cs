using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogSearchOverviewArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<WatchdogSearch> watchdogSearchRepository
) : BaseNhibernateQueryHandler<GetWatchdogSearchOverviewArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetWatchdogSearchOverviewArgsQuery query)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(query.WatchdogSearchId);
        return (TResult)(object)watchdogSearch.GetWatchdogSearchOverviewArgs();
    }
}