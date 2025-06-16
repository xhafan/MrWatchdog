using CoreUtils;
using MrWatchdog.Core.Messages;
using Rebus.Bus;
using Rebus.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

// registered as singleton
public class CoreBus(IBus bus) : ICoreBus
{
    public async Task Send(Command commandMessage)
    {
        Guard.Hope(commandMessage.Guid == Guid.Empty, "Command message Guid is already set.");
        commandMessage.Guid = Guid.NewGuid();
        await bus.Send(commandMessage, new Dictionary<string, string> {{Headers.MessageId, commandMessage.Guid.ToString()}});
    }
}