using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreDdd.Queries;
using CoreUtils.Extensions;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Account.Queries;
using System.Globalization;
using System.Security.Claims;
using CoreBackend.Account.Features.LoginLink;
using CoreWeb.Account.Features.LoginLink.CompleteLogin;

namespace MrWatchdog.Web.Features.Account.CompleteLogin;

public class CompleteLoginController(
    ICoreBus bus,
    IJobCompletionAwaiter jobCompletionAwaiter,
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) 
    : BaseCompleteLoginController(bus, jobCompletionAwaiter, queryExecutor, iJwtOptions)
{
    private readonly ICoreBus _bus = bus;
    private readonly IJobCompletionAwaiter _jobCompletionAwaiter = jobCompletionAwaiter;
    private readonly IQueryExecutor _queryExecutor = queryExecutor;

    protected override async Task<long> _FetchOrCreateUser(string email, ClaimsPrincipal? tokenClaimsPrincipal = null)
    {
        var cultureName = tokenClaimsPrincipal?.FindFirstValue(CustomClaimTypes.CultureName);
        var culture = cultureName != null 
            ? CultureInfo.GetCultureInfo(cultureName) 
            : CultureInfo.CurrentUICulture;

        var users = await _queryExecutor.ExecuteAsync<GetUserByEmailQuery, UserDto>(new GetUserByEmailQuery(email));
        if (users.IsEmpty())
        {
            var createUserCommand = new CreateUserCommand(email, culture);
            await _bus.Send(createUserCommand);
            await _jobCompletionAwaiter.WaitForJobCompletion(createUserCommand.Guid);
        }

        var user = (
            await _queryExecutor.ExecuteAsync<GetUserByEmailQuery, UserDto>(new GetUserByEmailQuery(email))
        ).Single();

        return user.Id;
    }
}