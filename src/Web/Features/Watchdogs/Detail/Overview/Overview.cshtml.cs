using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;
using Rebus.Bus;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

public class OverviewModel(IQueryExecutor queryExecutor, IBus bus) : BasePageModel
{
    [BindProperty]
    public WatchdogOverviewArgs WatchdogOverviewArgs { get; set; } = null!;

    public async Task OnGet(long id)
    {
        WatchdogOverviewArgs = (
            await queryExecutor.ExecuteAsync<GetWatchdogOverviewArgsQuery, WatchdogOverviewArgs>(new GetWatchdogOverviewArgsQuery(id))
        ).Single();
    }
    
    public async Task<IActionResult> OnPost(long id)
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogOverviewCommand(WatchdogOverviewArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}