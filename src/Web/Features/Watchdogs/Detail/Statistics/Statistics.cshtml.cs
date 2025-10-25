using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Statistics;

public class StatisticsModel(
    IQueryExecutor queryExecutor, 
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public long WatchdogId { get; private set; }
    public PublicWatchdogStatisticsDto PublicWatchdogStatistics { get; private set; } = null!;
    public PublicStatus WatchdogPublicStatus { get; private set; }

    public async Task<IActionResult> OnGet(long watchdogId)
    {
        WatchdogId = watchdogId;

        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId)) return Forbid();

        var statisticsResults = (
            await queryExecutor.ExecuteAsync<GetPublicWatchdogStatisticsQuery, GetPublicWatchdogStatisticsQueryResult>(
                new GetPublicWatchdogStatisticsQuery(watchdogId))
        ).ToList();

        PublicWatchdogStatistics = new PublicWatchdogStatisticsDto
        {
            CalculatedEarningsForThisMonth = 0,
            NumberOfUsersWithWatchdogSearchWithNotification = statisticsResults.SingleOrDefault(x => x.ReceiveNotification)?.CountOfWatchdogSearches ?? 0,
            NumberOfUsersWithWatchdogSearchWithoutNotification = statisticsResults.SingleOrDefault(x => !x.ReceiveNotification)?.CountOfWatchdogSearches ?? 0
        };

        var watchdogDetailPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailPublicStatusArgsQuery, WatchdogDetailPublicStatusArgs>(
                new GetWatchdogDetailPublicStatusArgsQuery(watchdogId));
        WatchdogPublicStatus = watchdogDetailPublicStatusArgs.PublicStatus;

        return Page();
    }
}