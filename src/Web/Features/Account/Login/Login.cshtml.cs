using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Features.Jobs.Queries;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Validations;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Features.Shared.ReinforcedTypings;

namespace MrWatchdog.Web.Features.Account.Login;

public class LoginModel(
    ICoreBus bus,
    IJobCompletionAwaiter jobCompletionAwaiter,
    IQueryExecutor queryExecutor
) : BasePageModel
{
    [BindProperty]
    [EmailAddressRegex(acceptSpacesAroundEmail: true)]
    public string Email { get; set; } = null!;
    
    [BindProperty]
    public string? ReturnUrl { get; set; }
    
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
}