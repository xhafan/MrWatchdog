using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Badges;

public class BadgesModel(
    IQueryExecutor queryExecutor,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public WatchdogDetailArgs WatchdogDetailArgs { get; set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public bool ShowPrivate { get; set; } = true;

    public async Task<IActionResult> OnGet(long watchdogId)
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId)) return Forbid();

        WatchdogDetailArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailArgsQuery, WatchdogDetailArgs>(
                new GetWatchdogDetailArgsQuery(watchdogId));

        return Page();
    }
}