using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Actions;

public class ActionsModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus
) : BasePageModel
{
    [BindProperty]
    public WatchdogDetailPublicStatusArgs WatchdogDetailPublicStatusArgs { get; set; } = null!;
    
    public async Task OnGet(long watchdogId)
    {
        WatchdogDetailPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailPublicStatusArgsQuery, WatchdogDetailPublicStatusArgs>(
                new GetWatchdogDetailPublicStatusArgsQuery(watchdogId));
    }
    
    public async Task<IActionResult> OnPostRequestToMakePublic(long watchdogId)
    {
        var command = new RequestToMakeWatchdogPublicCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    } 
    
    public async Task<IActionResult> OnPostMakePrivate(long watchdogId)
    {
        var command = new MakeWatchdogPrivateCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}