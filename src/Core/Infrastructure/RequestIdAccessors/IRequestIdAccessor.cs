namespace MrWatchdog.Core.Infrastructure.RequestIdAccessors;

public interface IRequestIdAccessor
{
    string? GetRequestId();
}