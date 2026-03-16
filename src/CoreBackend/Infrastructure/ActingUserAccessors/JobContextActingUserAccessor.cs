using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Core.Infrastructure.ActingUserAccessors;

public class JobContextActingUserAccessor : IActingUserAccessor
{
    public long GetActingUserId()
    {
        return JobContext.ActingUserId.Value;
    }
}