using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Account.Commands;

public class CreateUserCommandMessageHandler(
    IUserRepository userRepository
) 
    : IHandleMessages<CreateUserCommand>
{
    public async Task Handle(CreateUserCommand command)
    {
        var user = new User(command.Email);
        await userRepository.SaveAsync(user);
    }
}