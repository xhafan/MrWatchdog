using Microsoft.Extensions.Options;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Resources;

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

        var tokenString = TokenGenerator.GenerateToken(loginTokenGuid, command.Email, command.ReturnUrl, jwtOptions);

        var loginToken = new LoginToken(loginTokenGuid, command.Email, tokenString);
        await loginTokenRepository.SaveAsync(loginToken);
        
        var tokenParam = Uri.EscapeDataString(tokenString);
        var accountConfirmLoginUrl = $"{runtimeOptions.Url}{AccountUrlConstants.AccountConfirmLoginUrlTemplate.WithToken(tokenParam)}";

        var mrWatchdogResource = Resource.MrWatchdog;
        await bus.Send(new SendEmailCommand(
            command.Email,
            $"{mrWatchdogResource} login link",
            $"""
              <html>
              <body>
              <p>
                 If you just requested to log in to <a href="{runtimeOptions.Url}">{mrWatchdogResource}</a>, click the link below:
              </p>
              <p>
                 <a href="{accountConfirmLoginUrl}">{accountConfirmLoginUrl}</a>
              </p>
              <p>
                 This link expires in {jwtOptions.ExpireMinutes} minutes.
              </p>
              </body>
              </html>
              """
        ));
    }
}