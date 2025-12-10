using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Scrapers.Queries;

public class GetWatchdogSearchArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<WatchdogSearch> watchdogSearchRepository
) : BaseNhibernateQueryHandler<GetWatchdogSearchArgsQuery>(unitOfWork)
{
    public override async Task<TResult> ExecuteSingleAsync<TResult>(GetWatchdogSearchArgsQuery query)
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(query.WatchdogSearchId);
        return (TResult)(object)watchdogSearch.GetWatchdogSearchArgs();
    }
}