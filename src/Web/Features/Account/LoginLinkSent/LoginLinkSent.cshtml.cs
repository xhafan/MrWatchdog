using CoreDdd.Queries;
using CoreUtils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Web.Features.Shared;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace MrWatchdog.Web.Features.Account.LoginLinkSent;

[AllowAnonymous]
public class LoginLinkSentModel(
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) : BasePageModel
{
    [BindProperty(SupportsGet = true)]
    [Required]
    [NotDefault]
    public Guid LoginTokenGuid { get; set; }

    public string Email { get; private set; } = null!;

    public async Task<IActionResult> OnGet()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        var loginTokenDto = await queryExecutor.ExecuteSingleAsync<GetLoginTokenByGuidQuery, LoginTokenDto>(new GetLoginTokenByGuidQuery(LoginTokenGuid));

        var claimsPrincipal = TokenValidator.ValidateToken(loginTokenDto.Token, iJwtOptions.Value);
        
        var email = claimsPrincipal.FindFirstValue(ClaimTypes.Email);
        Guard.Hope(!string.IsNullOrWhiteSpace(email), "Cannot get email from token.");
        Email = email;

        return Page();
    }
}