using CoreBackend.Infrastructure.Configurations;
using CoreBackend.Infrastructure.EmailSenders;
using CoreBackend.Infrastructure.Rebus;
using Microsoft.Extensions.Options;
using MrWatchdog.Core.Resources;
using System.Security.Claims;
using CoreBackend.Account.Features.Account;
using CoreBackend.Account.Features.Account.Commands;
using CoreBackend.Account.Infrastructure.Repositories;

namespace MrWatchdog.Core.Features.Account.Commands;

public class SendLoginLinkToUserCommandMessageHandler(
    ILoginTokenRepository loginTokenRepository,
    ICoreBus bus,
    IOptions<JwtOptions> iJwtOptions,
    IOptions<RuntimeOptions> iRuntimeOptions
) 
    : BaseSendLoginLinkToUserCommandMessageHandler<SendLoginLinkToUserCommand>(
        loginTokenRepository,
        iJwtOptions,
        iRuntimeOptions
    )
{
    private readonly JwtOptions _jwtOptions = iJwtOptions.Value;
    private readonly RuntimeOptions _runtimeOptions = iRuntimeOptions.Value;

    protected override IEnumerable<Claim> GetCustomClaimForLoginTokenGeneration(SendLoginLinkToUserCommand command)
    {
        return [new Claim(CustomClaimTypes.CultureName, command.Culture.Name)];
    }

    protected override string GetAccountConfirmLoginUrl(string loginToken)
    {
        return AccountUrlConstants.AccountConfirmLoginUrlTemplate.WithLoginToken(loginToken);
    }

    protected override async Task SendLoginLinkToUser(SendLoginLinkToUserCommand command, string accountConfirmLoginUrl)
    {
        var mrWatchdogResource = ResourceHelper.GetString(nameof(Resource.MrWatchdog), command.Culture);

        await bus.Send(new SendEmailCommand(
            command.Email,
            string.Format(ResourceHelper.GetString(nameof(Resource.LoginLinkEmailSubject), command.Culture), mrWatchdogResource),
            string.Format(
                ResourceHelper.GetString(nameof(Resource.LoginLinkEmailBody), command.Culture),
                _runtimeOptions.Url,
                mrWatchdogResource,
                accountConfirmLoginUrl,
                _jwtOptions.ExpireMinutes
            )
        ));
    }
}