using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.Core.Resources;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Account.Commands;

public class SendLoginLinkToUserCommandMessageHandler(
    ILoginTokenRepository loginTokenRepository,
    ICoreBus bus,
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

        var loginTokenString = TokenGenerator.GenerateLoginToken(
            loginTokenGuid,
            command.Email,
            command.Culture.Name,
            command.ReturnUrl,
            jwtOptions
        );

        var loginToken = new LoginToken(loginTokenGuid, command.Email, loginTokenString);
        await loginTokenRepository.SaveAsync(loginToken);
        
        var tokenParam = Uri.EscapeDataString(loginTokenString);
        var accountConfirmLoginUrl = $"{runtimeOptions.Url}{AccountUrlConstants.AccountConfirmLoginUrlTemplate.WithToken(tokenParam)}";

        var mrWatchdogResource = ResourceHelper.GetString(nameof(Resource.MrWatchdog), command.Culture);

        await bus.Send(new SendEmailCommand(
            command.Email,
            string.Format(ResourceHelper.GetString(nameof(Resource.LoginLinkEmailSubject), command.Culture), mrWatchdogResource),
            string.Format(
                ResourceHelper.GetString(nameof(Resource.LoginLinkEmailBody), command.Culture),
                runtimeOptions.Url,
                mrWatchdogResource,
                accountConfirmLoginUrl,
                jwtOptions.ExpireMinutes
            )
        ));
    }
}