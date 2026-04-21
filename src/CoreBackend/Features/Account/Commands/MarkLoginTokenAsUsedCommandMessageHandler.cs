using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Account.Commands;

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