using CoreDdd.Queries;
using CoreUtils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;
using System.Security.Claims;

namespace MrWatchdog.Web.Features.Account.ConfirmLogin;

[AllowAnonymous]
public class ConfirmLoginModel(
    ICoreBus bus,
    IOptions<JwtOptions> iJwtOptions,
    IQueryExecutor queryExecutor,
    IJobCompletionAwaiter jobCompletionAwaiter
) : BasePageModel
{
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = null!;    
    
    public async Task<IActionResult> OnGet()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var claimsPrincipal = TokenValidator.ValidateToken(Uri.UnescapeDataString(Token), iJwtOptions.Value);

        var loginTokenGuidString = claimsPrincipal.FindFirstValue(CustomClaimTypes.Guid);
        Guard.Hope(!string.IsNullOrWhiteSpace(loginTokenGuidString), "Cannot get guid from token.");
        var loginTokenGuid = Guid.Parse(loginTokenGuidString);
        
        var loginTokenDto = await queryExecutor.ExecuteSingleAsync<GetLoginTokenByGuidQuery, LoginTokenDto>(new GetLoginTokenByGuidQuery(loginTokenGuid));
        Guard.Hope(!loginTokenDto.Confirmed, "Login token has already been confirmed.");
        
        var command = new ConfirmLoginTokenCommand(loginTokenGuid);
        await bus.Send(command);
        await jobCompletionAwaiter.WaitForJobCompletion(command.Guid);

        return Page();
    }
}