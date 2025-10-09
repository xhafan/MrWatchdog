using AspNetCore.ReCaptcha;
using CoreDdd.Queries;
using CoreUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Account.Login;

[ValidateReCaptcha]
[AllowAnonymous]
public class LoginModel(
    ICoreBus bus,
    IJobCompletionAwaiter jobCompletionAwaiter,
    IQueryExecutor queryExecutor
) : BasePageModel
{
    [BindProperty]
    [EmailAddressRegex(acceptSpacesAroundEmail: true)]
    public string Email { get; set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }
    
    // this field is here just to see ReCaptcha validation error
    public string Recaptcha { get; set; } = null!;

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        Email = Email.Trim();

        var command = new SendLoginLinkToUserCommand(Email, Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : null);
        await bus.Send(command);
        await jobCompletionAwaiter.WaitForJobCompletion(command.Guid);

        var jobDto = (
            await queryExecutor.ExecuteAsync<GetJobQuery, JobDto>(new GetJobQuery(command.Guid))
        ).Single();
        var loginTokenId = jobDto.AffectedEntities.Single(x => x.EntityName == DomainConstants.AccountLoginTokenEntityName).EntityId;
        var loginTokenDto = await queryExecutor.ExecuteSingleAsync<GetLoginTokenByIdQuery, LoginTokenDto>(new GetLoginTokenByIdQuery(loginTokenId));
        
        return Redirect(AccountUrlConstants.AccountLoginLinkSentUrlTemplate.WithLoginTokenGuid(loginTokenDto.Guid));
    }

    public IActionResult OnGetExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action("CompleteExternalLoginCallback", "CompleteLogin", values: new {returnUrl});
        Guard.Hope(redirectUrl != null, nameof(redirectUrl) + " is null");

        var properties = new AuthenticationProperties {RedirectUri = redirectUrl};
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            properties.Items.Add(AccountUrlConstants.ReturnUrl, returnUrl);
        }

        return new ChallengeResult(provider, properties);
    }
}