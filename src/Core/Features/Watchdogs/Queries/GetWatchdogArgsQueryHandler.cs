using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<GetWatchdogArgsQuery, WatchdogArgs>(unitOfWork)
{
    public override async Task<WatchdogArgs> ExecuteSingleAsync(GetWatchdogArgsQuery query)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(query.WatchdogId);
        return watchdog.GetWatchdogArgs();
    }
}