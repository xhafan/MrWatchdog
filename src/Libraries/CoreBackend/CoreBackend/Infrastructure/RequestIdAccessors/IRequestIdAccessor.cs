namespace CoreBackend.Infrastructure.RequestIdAccessors;

public interface IRequestIdAccessor
{
    string? GetRequestId();
}