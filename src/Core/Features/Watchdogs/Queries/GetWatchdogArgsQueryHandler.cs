using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<GetWatchdogArgsQuery>(unitOfWork)
{
    public override async Task<IEnumerable<TResult>> ExecuteAsync<TResult>(GetWatchdogArgsQuery argsQuery)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(argsQuery.WatchdogId);
        return (IEnumerable<TResult>) new List<WatchdogArgs> {watchdog.GetWatchdogArgs()};
    }
}