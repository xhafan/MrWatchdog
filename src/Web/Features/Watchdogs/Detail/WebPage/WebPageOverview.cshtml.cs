using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageOverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BasePageModel
{
    [BindProperty]
    public WatchdogWebPageArgs WatchdogWebPageArgs { get; set; } = null!;
    public bool IsEmptyWebPage { get; private set; }

    public async Task<IActionResult> OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }

        WatchdogWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageArgsQuery, WatchdogWebPageArgs>(
                new GetWatchdogWebPageArgsQuery(watchdogId, watchdogWebPageId));

        IsEmptyWebPage = string.IsNullOrWhiteSpace(WatchdogWebPageArgs.Url);

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!(await authorizationService.AuthorizeAsync(User, WatchdogWebPageArgs.WatchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogWebPageCommand(WatchdogWebPageArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}