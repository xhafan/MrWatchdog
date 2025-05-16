using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;
using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogCommandMessageHandler(IRepository<Watchdog> watchdogRepository) : IHandleMessages<CreateWatchdogCommand>
{
    public async Task Handle(CreateWatchdogCommand command)
    {
        var newWatchdog = new Watchdog(command.Name);
        await watchdogRepository.SaveAsync(newWatchdog);
    }
}