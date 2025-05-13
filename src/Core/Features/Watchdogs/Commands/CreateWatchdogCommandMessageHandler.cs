using Rebus.Handlers;

namespace MrWatchdog.Core.Features.Watchdogs.Commands;

public class CreateWatchdogCommandMessageHandler : IHandleMessages<CreateWatchdogCommand> // todo: test me
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task Handle(CreateWatchdogCommand command)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
    }
}