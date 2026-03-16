using CoreBackend.Messages;

namespace CoreBackend.Infrastructure.Rebus;

public interface ICoreBus
{
    Task Send(Command commandMessage);
}