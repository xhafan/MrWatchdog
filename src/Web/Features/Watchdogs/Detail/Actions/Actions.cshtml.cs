using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Actions;

public class ActionsModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public WatchdogDetailPublicStatusArgs WatchdogDetailPublicStatusArgs { get; set; } = null!;
    
    public async Task<IActionResult> OnGet(long watchdogId)
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId)) return Forbid();

        WatchdogDetailPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailPublicStatusArgsQuery, WatchdogDetailPublicStatusArgs>(
                new GetWatchdogDetailPublicStatusArgsQuery(watchdogId));

        return Page();
    }
    
    public async Task<IActionResult> OnPostRequestToMakePublic(long watchdogId)
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId)) return Forbid();

        var command = new RequestToMakeWatchdogPublicCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    } 
    
    public async Task<IActionResult> OnPostMakePrivate(long watchdogId)
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId)) return Forbid();

        var command = new MakeWatchdogPrivateCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}