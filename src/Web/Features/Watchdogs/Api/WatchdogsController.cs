using System.Security.Claims;
using CoreDdd.Queries;
using CoreUtils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Web.Features.Watchdogs.Api;

[ApiController]
[Route("api/[controller]")]
public class WatchdogsController(
    ICoreBus bus,
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) : ControllerBase
{
    [HttpPost("[action]")]
    public async Task<IActionResult> DisableNotification(string unsubscribeToken)
    {
        var claimsPrincipal = TokenValidator.ValidateToken(Uri.UnescapeDataString(unsubscribeToken), iJwtOptions.Value, validateLifetime: false);

        var watchdogIdString = claimsPrincipal.FindFirstValue(CustomClaimTypes.WatchdogId);
        Guard.Hope(!string.IsNullOrWhiteSpace(watchdogIdString), "Cannot get watchdogId from token.");
        var watchdogId = long.Parse(watchdogIdString);

        var watchdogExists = await queryExecutor.ExecuteSingleAsync<DoesWatchdogExitsQuery, bool>(new DoesWatchdogExitsQuery(watchdogId));

        if (!watchdogExists) return NotFound();

        var command = new DisableWatchdogNotificationCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}