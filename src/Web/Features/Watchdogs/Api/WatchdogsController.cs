using CoreDdd.Queries;
using CoreUtils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using System.Security.Claims;

namespace MrWatchdog.Web.Features.Watchdogs.Api;

[ApiController]
[Route("api/[controller]")]
public class WatchdogsController(
    ICoreBus bus,
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("[action]")]
    public Task<IActionResult> DisableNotificationPost(string unsubscribeToken)
        => HandleDisableNotificationAsync(unsubscribeToken, redirectOnSuccess: false);

    [AllowAnonymous]
    [HttpGet("[action]")]
    public Task<IActionResult> DisableNotificationGet(string unsubscribeToken)
        => HandleDisableNotificationAsync(unsubscribeToken, redirectOnSuccess: true);

    private async Task<IActionResult> HandleDisableNotificationAsync(string unsubscribeToken, bool redirectOnSuccess)
    {
        var claimsPrincipal = TokenValidator.ValidateToken(
            Uri.UnescapeDataString(unsubscribeToken),
            iJwtOptions.Value,
            validateLifetime: false
        );

        var watchdogIdString = claimsPrincipal.FindFirstValue(CustomClaimTypes.WatchdogId);
        Guard.Hope(!string.IsNullOrWhiteSpace(watchdogIdString), "Cannot get watchdogId from token.");
        if (!long.TryParse(watchdogIdString, out var watchdogId))
        {
            throw new InvalidOperationException("WatchdogId in token is not a valid long.");
        }

        var watchdogExists = await queryExecutor.ExecuteSingleAsync<DoesWatchdogExitsQuery, bool>(
            new DoesWatchdogExitsQuery(watchdogId)
        );

        if (!watchdogExists) return NotFound();

        var command = new DisableWatchdogNotificationCommand(watchdogId);
        await bus.Send(command);

        return redirectOnSuccess
            ? RedirectToPage("/Watchdogs/Unsubscribed/Unsubscribed")
            : Ok(command.Guid.ToString());
    }

}