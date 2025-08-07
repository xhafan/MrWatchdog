using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

public class DetailModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus
) : BasePageModel
{
    public WatchdogDetailArgs WatchdogDetailArgs { get; private set; } = null!;
    
    public async Task OnGet(long watchdogId)
    {
        WatchdogDetailArgs = await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailArgsQuery, WatchdogDetailArgs>(new GetWatchdogDetailArgsQuery(watchdogId));
    }
    
    public async Task<IActionResult> OnPostCreateWatchdogWebPage(long watchdogId)
    {
        var command = new CreateWatchdogWebPageCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
    
    public async Task<IActionResult> OnPostDeleteWatchdog(long watchdogId)
    {
        var command = new DeleteWatchdogCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}