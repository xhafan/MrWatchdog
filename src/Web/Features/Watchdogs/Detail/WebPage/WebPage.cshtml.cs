using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

[Authorize]
public class WebPageModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus
) : BasePageModel
{
    public long WatchdogId { get; private set; }
    public long WatchdogWebPageId { get; private set; }
    public string? WatchdogWebPageName { get; private set; }

    public async Task OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogId = watchdogId;
        WatchdogWebPageId = watchdogWebPageId;

        var watchdogWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageArgsQuery, WatchdogWebPageArgs>(
                new GetWatchdogWebPageArgsQuery(watchdogId, watchdogWebPageId));
        
        WatchdogWebPageName = watchdogWebPageArgs.Name;
    }
    
    public async Task<IActionResult> OnPostRemoveWatchdogWebPage(long watchdogId, long watchdogWebPageId)
    {
        var command = new RemoveWatchdogWebPageCommand(watchdogId, watchdogWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }     
}