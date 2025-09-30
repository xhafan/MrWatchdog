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

public class WebPageModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BasePageModel
{
    public long WatchdogId { get; private set; }
    public long WatchdogWebPageId { get; private set; }
    public string? WatchdogWebPageName { get; private set; }

    public async Task<IActionResult> OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }        

        WatchdogId = watchdogId;
        WatchdogWebPageId = watchdogWebPageId;

        var watchdogWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageArgsQuery, WatchdogWebPageArgs>(
                new GetWatchdogWebPageArgsQuery(watchdogId, watchdogWebPageId));
        
        WatchdogWebPageName = watchdogWebPageArgs.Name;

        return Page();
    }
    
    public async Task<IActionResult> OnPostRemoveWatchdogWebPage(long watchdogId, long watchdogWebPageId)
    {
        if (!(await authorizationService.AuthorizeAsync(User, watchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }  

        var command = new RemoveWatchdogWebPageCommand(watchdogId, watchdogWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }     
}