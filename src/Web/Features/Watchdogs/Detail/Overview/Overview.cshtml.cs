using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

public class OverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BasePageModel
{
    [BindProperty]
    public WatchdogOverviewArgs WatchdogOverviewArgs { get; set; } = null!;

    public async Task<IActionResult> OnGet(long watchdogId)
    {
        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }

        WatchdogOverviewArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogOverviewArgsQuery, WatchdogOverviewArgs>(
                new GetWatchdogOverviewArgsQuery(watchdogId));

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!(await authorizationService.AuthorizeAsync(User, WatchdogOverviewArgs.WatchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogOverviewCommand(WatchdogOverviewArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}