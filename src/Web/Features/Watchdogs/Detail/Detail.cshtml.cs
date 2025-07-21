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
    
    public async Task OnGet(long id)
    {
        WatchdogDetailArgs = await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailArgsQuery, WatchdogDetailArgs>(new GetWatchdogDetailArgsQuery(id));
    }
    
    public async Task<IActionResult> OnPostCreateWatchdogWebPage(long id)
    {
        var command = new CreateWatchdogWebPageCommand(WatchdogId: id);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}