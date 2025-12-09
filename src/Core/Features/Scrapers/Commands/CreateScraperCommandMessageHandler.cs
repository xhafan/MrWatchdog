using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogCommandMessageHandler(
    IRepository<Watchdog> watchdogRepository,
    IUserRepository userRepository
) 
    : IHandleMessages<CreateWatchdogCommand>
{
    public async Task Handle(CreateWatchdogCommand command)
    {
        var user = await userRepository.LoadByIdAsync(command.UserId);
        var newWatchdog = new Watchdog(user, command.Name);
        await watchdogRepository.SaveAsync(newWatchdog);
    }
}