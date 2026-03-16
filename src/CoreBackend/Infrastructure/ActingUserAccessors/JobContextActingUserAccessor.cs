using CoreBackend.Infrastructure.Rebus;

namespace CoreBackend.Infrastructure.ActingUserAccessors;

public class JobContextActingUserAccessor : IActingUserAccessor
{
    public long GetActingUserId()
    {
        return JobContext.ActingUserId.Value;
    }
}