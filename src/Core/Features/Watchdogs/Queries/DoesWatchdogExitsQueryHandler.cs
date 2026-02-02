using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class DoesWatchdogExitsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<DoesWatchdogExitsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(DoesWatchdogExitsQuery query)
    {
        var watchdog = await watchdogRepository.GetAsync(query.WatchdogId);
        return (TResult)(object)(watchdog != null);
    }
}