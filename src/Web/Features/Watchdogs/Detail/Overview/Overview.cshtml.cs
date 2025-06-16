using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

public class OverviewModel(IQueryExecutor queryExecutor, ICoreBus bus) : BasePageModel
{
    [BindProperty]
    public WatchdogOverviewArgs WatchdogOverviewArgs { get; set; } = null!;

    public async Task OnGet(long id)
    {
        WatchdogOverviewArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogOverviewArgsQuery, WatchdogOverviewArgs>(
                new GetWatchdogOverviewArgsQuery(id));
    }
    
    public async Task<IActionResult> OnPost()
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