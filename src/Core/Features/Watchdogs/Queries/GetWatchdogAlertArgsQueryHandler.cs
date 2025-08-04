using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogAlertArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<WatchdogAlert> watchdogAlertRepository
) : BaseNhibernateQueryHandler<GetWatchdogAlertArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetWatchdogAlertArgsQuery query)
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(query.WatchdogAlertId);
        return (TResult)(object)watchdogAlert.GetWatchdogAlertArgs();
    }
}