using CoreDdd.Queries;
using CoreUtils;
using CoreUtils.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Validations;

namespace MrWatchdog.Web.Features.Account.CompleteLogin;

[ApiController]
[Route("api/[controller]/[action]")]
[AllowAnonymous]
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

        var markLoginTokenAsUsedCommand = new MarkLoginTokenAsUsedCommand(loginTokenGuid);
        await bus.Send(markLoginTokenAsUsedCommand);
        await jobCompletionAwaiter.WaitForJobCompletion(markLoginTokenAsUsedCommand.Guid);

        var cultureName = tokenClaimsPrincipal.FindFirstValue(CustomClaimTypes.CultureName);
        Guard.Hope(!string.IsNullOrWhiteSpace(cultureName), "Cannot get culture name from token.");

        await _FetchOrCreateUserAndLogUserIn(email, CultureInfo.GetCultureInfo(cultureName));

        var returnUrl = tokenClaimsPrincipal.FindFirstValue(CustomClaimTypes.ReturnUrl);

        return Ok(
            !string.IsNullOrWhiteSpace(returnUrl)
                ? returnUrl
                : "/"
        );
    }

    private async Task _FetchOrCreateUserAndLogUserIn(string email, CultureInfo culture)
    {
        var users = await queryExecutor.ExecuteAsync<GetUserByEmailQuery, UserDto>(new GetUserByEmailQuery(email));
        if (users.IsEmpty())
        {
            var createUserCommand = new CreateUserCommand(email, culture);
            await bus.Send(createUserCommand);
            await jobCompletionAwaiter.WaitForJobCompletion(createUserCommand.Guid);
        }

        var user = (
            await queryExecutor.ExecuteAsync<GetUserByEmailQuery, UserDto>(new GetUserByEmailQuery(email))
        ).Single();

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
    }

    [HttpGet]
    public async Task<IActionResult> CompleteExternalLoginCallback(string? returnUrl = null)
    {
        var provider = User.Identity?.AuthenticationType;
        var isAuthenticatedByExternalProvider = User.Identity?.IsAuthenticated == true
                                                && provider != null
                                                && provider != CookieAuthenticationDefaults.AuthenticationScheme;
        if (!isAuthenticatedByExternalProvider)
        {
            return Unauthorized();
        }

        var email = User.FindFirstValue(ClaimTypes.Email);
        Guard.Hope(!string.IsNullOrWhiteSpace(email), $"Cannot get email from the external provider {provider}.");

        await _FetchOrCreateUserAndLogUserIn(email, CultureInfo.CurrentUICulture);

        return Redirect(
            !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : "/"
        );
    }
}