using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.Statistics;

public class StatisticsModel(
    IQueryExecutor queryExecutor, 
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public long ScraperId { get; private set; }
    public PublicScraperStatisticsDto PublicScraperStatistics { get; private set; } = null!;
    public PublicStatus ScraperPublicStatus { get; private set; }

    public async Task<IActionResult> OnGet(long scraperId)
    {
        ScraperId = scraperId;

        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        var statisticsResults = (
            await queryExecutor.ExecuteAsync<GetPublicScraperStatisticsQuery, GetPublicScraperStatisticsQueryResult>(
                new GetPublicScraperStatisticsQuery(scraperId))
        ).ToList();

        PublicScraperStatistics = new PublicScraperStatisticsDto
        {
            CalculatedEarningsForThisMonth = 0,
            NumberOfUsersWithWatchdogWithNotification = statisticsResults.SingleOrDefault(x => x.ReceiveNotification)?.CountOfWatchdogSearches ?? 0,
            NumberOfUsersWithWatchdogWithoutNotification = statisticsResults.SingleOrDefault(x => !x.ReceiveNotification)?.CountOfWatchdogSearches ?? 0
        };

        var scraperDetailPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperDetailPublicStatusArgsQuery, ScraperDetailPublicStatusArgs>(
                new GetScraperDetailPublicStatusArgsQuery(scraperId));
        ScraperPublicStatus = scraperDetailPublicStatusArgs.PublicStatus;

        return Page();
    }
}