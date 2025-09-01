using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Features.Account.Commands;

public class SendLoginLinkToUserCommandMessageHandler(
    ILoginTokenRepository loginTokenRepository,
    IEmailSender emailSender,
    IOptions<JwtOptions> iJwtOptions,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : IHandleMessages<SendLoginLinkToUserCommand>
{
    public async Task Handle(SendLoginLinkToUserCommand command)
    {
        var jwtOptions = iJwtOptions.Value;
        var runtimeOptions = iRuntimeOptions.Value;

        var loginTokenGuid = Guid.NewGuid();

        var tokenString = TokenGenerator.GenerateToken(loginTokenGuid, command.Email, command.ReturnUrl, jwtOptions);

        var loginToken = new LoginToken(loginTokenGuid, command.Email, tokenString);
        await loginTokenRepository.SaveAsync(loginToken);
        
        var tokenParam = Uri.EscapeDataString(tokenString);
        var accountConfirmLoginUrl = $"{runtimeOptions.Url}{AccountUrlConstants.AccountConfirmLoginUrl}".Replace(AccountUrlConstants.TokenVariable, tokenParam);

        var mrWatchdogResource = Resource.MrWatchdog;
        await emailSender.SendEmail(
            command.Email,
            $"{mrWatchdogResource} login link",
            $"""
             <p>
                If you just requested to log in to {mrWatchdogResource}, click <a href="{accountConfirmLoginUrl}">here</a>. 
             </p>
             <p>
                This link expires in {jwtOptions.ExpireMinutes} minutes.
             </p>
             """
        );
    }
}