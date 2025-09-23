using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Actions;

// This class exist due to Razor pages (ActionsModel) is not able to have superadmin policy for some handlers.
[ApiController]
[Route("api/Watchdogs/Detail/[controller]/[action]")]
public class ActionsController(ICoreBus bus) : ControllerBase
{
    [Authorize(Policies.SuperAdmin)]
    public async Task<IActionResult> MakePublic(long watchdogId)
    {
        var command = new MakeWatchdogPublicCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}