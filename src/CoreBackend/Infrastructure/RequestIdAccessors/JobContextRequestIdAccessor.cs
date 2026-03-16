using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Core.Infrastructure.RequestIdAccessors;

public class JobContextRequestIdAccessor : IRequestIdAccessor
{
    public string? GetRequestId()
    {
        return JobContext.RequestId.Value;
    }
}