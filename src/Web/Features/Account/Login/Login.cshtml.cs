using CoreBackend.Features.Account.Commands;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreBackend.Infrastructure.Validations;
using CoreDdd.Queries;
using CoreWeb.Features.Account.Login;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Resources;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using CoreBackend.Features.Account;
using MrWatchdog.Core.Features.Account;

namespace MrWatchdog.Web.Features.Account.Login;

public class LoginModel(
    ICoreBus bus,
    IJobCompletionAwaiter jobCompletionAwaiter,
    IQueryExecutor queryExecutor
) 
    : BaseLoginModel(bus, jobCompletionAwaiter, queryExecutor)
{
    [BindProperty]
    [Required]
    [EmailAddressRegex(acceptSpacesAroundEmail: true)]
    [StringLength(254)]
    [Display(Name = nameof(Resource.Email), ResourceType = typeof(Resource))]
    public string Email { get; set; } = null!;

    protected override BaseSendLoginLinkToUserCommand GetSendLoginLinkToUserCommand()
    {
        return new SendLoginLinkToUserCommand(
            Email.Trim(),
            CultureInfo.CurrentUICulture,
            Url.IsLocalUrl(ReturnUrl) ? ReturnUrl : null
        );
    }

    protected override string GetAccountLoginLinkSentUrl(Guid loginTokenGuid)
    {
        return AccountUrlConstants.AccountLoginLinkSentUrlTemplate.WithLoginTokenGuid(loginTokenGuid);
    }
}