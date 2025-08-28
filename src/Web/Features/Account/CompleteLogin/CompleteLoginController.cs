using System.ComponentModel.DataAnnotations;
using CoreDdd.Queries;
using CoreUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using System.Security.Claims;
using CoreUtils.Extensions;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Web.Features.Account.CompleteLogin;

[ApiController]
[Route("api/[controller]")]
public class CompleteLoginController(
    ICoreBus bus,
    IJobCompletionAwaiter jobCompletionAwaiter,
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CompleteLogin([Required, NotDefault]Guid loginTokenGuid)
    {
        var loginTokenDto = await queryExecutor.ExecuteSingleAsync<GetLoginTokenByGuidQuery, LoginTokenDto>(new GetLoginTokenByGuidQuery(loginTokenGuid));

        Guard.Hope(loginTokenDto.Confirmed, "Login token has not been confirmed.");
        Guard.Hope(!loginTokenDto.Used, "Login token has been already used.");
        
        var tokenClaimsPrincipal = TokenValidator.ValidateToken(loginTokenDto.Token, iJwtOptions.Value);
        
        var email = tokenClaimsPrincipal.FindFirstValue(ClaimTypes.Email);
        Guard.Hope(!string.IsNullOrWhiteSpace(email), "Cannot get email from token.");

        var users = await queryExecutor.ExecuteAsync<GetUserByEmailQuery, UserDto>(new GetUserByEmailQuery(email));
        if (users.IsEmpty())
        {
            var createUserCommand = new CreateUserCommand(email);
            await bus.Send(createUserCommand);
            await jobCompletionAwaiter.WaitForJobCompletion(createUserCommand.Guid);
        }

        var user = (
            await queryExecutor.ExecuteAsync<GetUserByEmailQuery, UserDto>(new GetUserByEmailQuery(email))
        ).Single();

        var markLoginTokenAsUsedCommand = new MarkLoginTokenAsUsedCommand(loginTokenGuid);
        await bus.Send(markLoginTokenAsUsedCommand);
        await jobCompletionAwaiter.WaitForJobCompletion(markLoginTokenAsUsedCommand.Guid);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var cookieClaimsPrincipal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            cookieClaimsPrincipal,
            new AuthenticationProperties {IsPersistent = true}
        );

        var returnUrl = tokenClaimsPrincipal.FindFirstValue(CustomClaimTypes.ReturnUrl);

        return Redirect(
            !string.IsNullOrWhiteSpace(returnUrl)
                ? returnUrl
                : "/"
        );
    }    
}