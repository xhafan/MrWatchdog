using System.Security.Claims;
using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreBackend.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Rebus.Handlers;

namespace CoreBackend.Account.Features.LoginLink.Commands;

public abstract class BaseSendLoginLinkToUserCommandMessageHandler<TSendLoginLinkToUserCommand>(
    ILoginTokenRepository loginTokenRepository,
    IOptions<JwtOptions> iJwtOptions,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<TSendLoginLinkToUserCommand> where TSendLoginLinkToUserCommand : BaseSendLoginLinkToUserCommand
{
    protected abstract IEnumerable<Claim> GetCustomClaimForLoginTokenGeneration(TSendLoginLinkToUserCommand command);
    protected abstract string GetAccountConfirmLoginUrl(string loginToken);
    protected abstract Task SendLoginLinkToUser(TSendLoginLinkToUserCommand command, string accountConfirmLoginUrl);

    public async Task Handle(TSendLoginLinkToUserCommand command)
    {
        var jwtOptions = iJwtOptions.Value;
        var runtimeOptions = iRuntimeOptions.Value;

        var loginTokenGuid = Guid.NewGuid();

        var loginTokenString = TokenGenerator.GenerateLoginToken(
            loginTokenGuid,
            command.Email,
            GetCustomClaimForLoginTokenGeneration(command),
            command.ReturnUrl,
            jwtOptions
        );

        var loginToken = new LoginToken(loginTokenGuid, command.Email, loginTokenString);
        await loginTokenRepository.SaveAsync(loginToken);
        
        var accountConfirmLoginUrl = $"{runtimeOptions.Url}{GetAccountConfirmLoginUrl(Uri.EscapeDataString(loginTokenString))}";

        await SendLoginLinkToUser(command, accountConfirmLoginUrl);
    }
}