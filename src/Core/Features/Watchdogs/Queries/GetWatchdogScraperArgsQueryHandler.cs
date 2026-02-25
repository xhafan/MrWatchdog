using CoreDdd.Domain.Repositories;
using CoreDdd.Nhibernate.Queries;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Queries;

public class GetWatchdogScraperArgsQueryHandler(
    NhibernateUnitOfWork unitOfWork, 
    IRepository<Watchdog> watchdogRepository
) : BaseNhibernateQueryHandler<GetWatchdogScraperArgsQuery, WatchdogScraperDto>(unitOfWork)
{
    public override async Task<WatchdogScraperDto> ExecuteSingleAsync(GetWatchdogScraperArgsQuery query)
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(query.WatchdogId);
        return watchdog.GetWatchdogScraperDto();
    }
}