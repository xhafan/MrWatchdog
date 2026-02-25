using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class DoesWatchdogExitsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<DoesWatchdogExitsQuery, bool>(unitOfWork)
{
    public override async Task<bool> ExecuteSingleAsync(DoesWatchdogExitsQuery query)
    {
        var watchdog = await watchdogRepository.GetAsync(query.WatchdogId);
        return watchdog != null;
    }
}