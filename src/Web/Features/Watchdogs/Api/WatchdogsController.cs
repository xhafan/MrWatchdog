using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Web.Features.Watchdogs.Api;

[ApiController]
[Route("api/[controller]")]
public class WatchdogsController(
    ICoreBus bus,
    IQueryExecutor queryExecutor
    ) : ControllerBase
{
    [HttpPost("{watchdogId}/[action]")]
    public async Task<IActionResult> DisableNotification(long watchdogId)
    {
        var watchdogExists = await queryExecutor.ExecuteSingleAsync<DoesWatchdogExitsQuery, bool>(new DoesWatchdogExitsQuery(watchdogId));

        if (!watchdogExists) return NotFound();

        var command = new DisableWatchdogNotificationCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}