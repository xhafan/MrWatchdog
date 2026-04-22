using CoreBackend.Infrastructure.Rebus;

namespace CoreBackend.Infrastructure.RequestIdAccessors;

public class JobContextRequestIdAccessor : IRequestIdAccessor
{
    public string? GetRequestId()
    {
        return JobContext.RequestId.Value;
    }
}