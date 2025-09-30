using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

public class DetailModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService

) : BasePageModel
{
    public WatchdogDetailArgs WatchdogDetailArgs { get; private set; } = null!;
    
    public async Task<IActionResult> OnGet(long watchdogId)
    {
        WatchdogDetailArgs = await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailArgsQuery, WatchdogDetailArgs>(
            new GetWatchdogDetailArgsQuery(watchdogId)
        );

        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return WatchdogDetailArgs.PublicStatus == PublicStatus.Public 
                ? Redirect(WatchdogUrlConstants.WatchdogScrapingResultsUrlTemplate.WithWatchdogId(watchdogId))
                : Forbid();
        }

        return Page();
    }
    
    public async Task<IActionResult> OnPostCreateWatchdogWebPage(long watchdogId)
    {
        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }

        var command = new CreateWatchdogWebPageCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
    
    public async Task<IActionResult> OnPostDeleteWatchdog(long watchdogId)
    {
        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }

        var command = new DeleteWatchdogCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}