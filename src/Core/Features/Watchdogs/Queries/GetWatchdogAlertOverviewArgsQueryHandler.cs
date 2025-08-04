using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogAlertOverviewArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<WatchdogAlert> watchdogAlertRepository
) : BaseNhibernateQueryHandler<GetWatchdogAlertOverviewArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetWatchdogAlertOverviewArgsQuery query)
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(query.WatchdogAlertId);
        return (TResult)(object)watchdogAlert.GetWatchdogAlertOverviewArgs();
    }
}