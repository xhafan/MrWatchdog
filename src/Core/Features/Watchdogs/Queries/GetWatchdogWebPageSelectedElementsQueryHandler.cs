using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogWebPageSelectedElementsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<GetWatchdogWebPageSelectedElementsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetWatchdogWebPageSelectedElementsQuery query)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(query.WatchdogId);
        return (TResult)(object)watchdog.GetWatchdogWebPageSelectedElementsDto(query.WatchdogWebPageId);
    }
}