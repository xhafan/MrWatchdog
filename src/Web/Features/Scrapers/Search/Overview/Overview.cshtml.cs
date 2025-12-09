using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Search.Overview;

public class OverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public WatchdogSearchOverviewArgs WatchdogSearchOverviewArgs { get; set; } = null!;

    public async Task<IActionResult> OnGet(long watchdogSearchId)
    {
        if (!await IsAuthorizedAsWatchdogSearchOwnerOrSuperAdmin(watchdogSearchId)) return Forbid();

        WatchdogSearchOverviewArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogSearchOverviewArgsQuery, WatchdogSearchOverviewArgs>(
                new GetWatchdogSearchOverviewArgsQuery(watchdogSearchId));

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!await IsAuthorizedAsWatchdogSearchOwnerOrSuperAdmin(WatchdogSearchOverviewArgs.WatchdogSearchId)) return Forbid();

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogSearchOverviewCommand(WatchdogSearchOverviewArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}