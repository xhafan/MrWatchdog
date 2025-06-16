using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogWebPageSelectedHtmlQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<GetWatchdogWebPageSelectedHtmlQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetWatchdogWebPageSelectedHtmlQuery query)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(query.WatchdogId);
        return (TResult)(object)watchdog.GetWatchdogWebPageArgs(query.WatchdogWebPageId);
    }
}