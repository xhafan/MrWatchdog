using CoreBackend.Account.Infrastructure.Repositories;
using Rebus.Handlers;

namespace CoreBackend.Account.Features.Account.Commands;

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