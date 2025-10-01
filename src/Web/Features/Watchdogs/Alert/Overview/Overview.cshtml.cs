using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Alert.Overview;

public class OverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public WatchdogAlertOverviewArgs WatchdogAlertOverviewArgs { get; set; } = null!;

    public async Task<IActionResult> OnGet(long watchdogAlertId)
    {
        if (!await IsAuthorizedAsWatchdogAlertOwnerOrSuperAdmin(watchdogAlertId)) return Forbid();

        WatchdogAlertOverviewArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogAlertOverviewArgsQuery, WatchdogAlertOverviewArgs>(
                new GetWatchdogAlertOverviewArgsQuery(watchdogAlertId));

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!await IsAuthorizedAsWatchdogAlertOwnerOrSuperAdmin(WatchdogAlertOverviewArgs.WatchdogAlertId)) return Forbid();

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogAlertOverviewCommand(WatchdogAlertOverviewArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}