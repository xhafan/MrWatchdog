using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Infrastructure.RateLimiting;

public static class HttpContextUserEmailOrClientIpAddressGetter
{
    public static string GetUserEmailOrClientIpAddress(HttpContext httpContext)
    {
        return httpContext.User.IsAuthenticated()
            ? $"{httpContext.User.Identity?.Name}"
            : $"[{httpContext.Connection.RemoteIpAddress}]";
    }
}
