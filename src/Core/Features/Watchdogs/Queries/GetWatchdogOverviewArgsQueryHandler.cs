using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogOverviewArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<GetWatchdogOverviewArgsQuery, WatchdogOverviewArgs>(unitOfWork)
{
    public override async Task<WatchdogOverviewArgs> ExecuteSingleAsync(GetWatchdogOverviewArgsQuery query)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(query.WatchdogId);
        return watchdog.GetWatchdogOverviewArgs();
    }
}