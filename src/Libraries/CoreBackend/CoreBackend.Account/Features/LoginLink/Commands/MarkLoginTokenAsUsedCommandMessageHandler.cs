using CoreBackend.Account.Infrastructure.Repositories;
using Rebus.Handlers;

namespace CoreBackend.Account.Features.LoginLink.Commands;

public class MarkLoginTokenAsUsedCommandMessageHandler(
    ILoginTokenRepository loginTokenRepository
) 
    : IHandleMessages<MarkLoginTokenAsUsedCommand>
{
    public async Task Handle(MarkLoginTokenAsUsedCommand command)
    {
        var loginToken = await loginTokenRepository.LoadByGuidAsync(command.LoginTokenGuid);
        loginToken.MarkAsUsed();
    }
}