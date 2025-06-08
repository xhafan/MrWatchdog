using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;
using Rebus.Bus;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageModel(
    IQueryExecutor queryExecutor, 
    IBus bus
) : BasePageModel
{
    [BindProperty]
    public WatchdogWebPageArgs WatchdogWebPageArgs { get; set; } = null!;

    public async Task OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageArgsQuery, WatchdogWebPageArgs>(
                new GetWatchdogWebPageArgsQuery(watchdogId, watchdogWebPageId));
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogWebPageCommand(WatchdogWebPageArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
    
    public async Task<IActionResult> OnPostRemoveWatchdogWebPage(long id, long watchdogWebPageId)
    {
        var command = new RemoveWatchdogWebPageCommand(WatchdogId: id, watchdogWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }     
}