using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Account.Commands;

public class ConfirmLoginTokenCommandMessageHandler(
    ILoginTokenRepository loginTokenRepository
) 
    : IHandleMessages<ConfirmLoginTokenCommand>
{
    public async Task Handle(ConfirmLoginTokenCommand command)
    {
        var loginToken = await loginTokenRepository.LoadByGuidAsync(command.LoginTokenGuid);
        loginToken.Confirm();
    }
}