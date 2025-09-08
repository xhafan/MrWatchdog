using CoreUtils;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.RequestIdAccessors;
using MrWatchdog.Core.Messages;
using Rebus.Bus;
using Rebus.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public class CoreBus(
    IBus bus, 
    IJobCreator jobCreator,
    IActingUserAccessor actingUserAccessor,
    IRequestIdAccessor requestIdAccessor
    
) : ICoreBus
{
    public async Task Send(Command commandMessage)
    {
        Guard.Hope(commandMessage.Guid == Guid.Empty, "Command message Guid is already set.");
        commandMessage.Guid = Guid.NewGuid();
        commandMessage.ActingUserId = actingUserAccessor.GetActingUserId();
        commandMessage.RequestId = requestIdAccessor.GetRequestId();

        await jobCreator.CreateJob(commandMessage, commandMessage.Guid, shouldMarkJobAsHandlingStarted: false);

        
        await bus.Send(commandMessage, new Dictionary<string, string> {{Headers.MessageId, commandMessage.Guid.ToString()}});
    }
}