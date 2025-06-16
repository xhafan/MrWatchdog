using MrWatchdog.Core.Messages;

namespace MrWatchdog.Core.Infrastructure.Rebus;

public interface ICoreBus
{
    Task Send(Command commandMessage);
}