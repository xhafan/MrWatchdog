using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[AllowAnonymous]
public class ScrapingResultsModel(
    IQueryExecutor queryExecutor,
    IActingUserAccessor actingUserAccessor,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    
    public async Task<IActionResult> OnGet(long watchdogId)
    {
        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(watchdogId)
            );

        if (WatchdogScrapingResultsArgs.PublicStatus == PublicStatus.Public)
        {
            return Page();
        }

        var isAuthenticatedUser = actingUserAccessor.GetActingUserId() != 0;
        if (!isAuthenticatedUser || !await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId))
        {
            return Forbid();
        }

        return Page();
    }
}