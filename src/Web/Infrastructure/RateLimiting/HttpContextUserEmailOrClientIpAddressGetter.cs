namespace MrWatchdog.Web.Infrastructure.RateLimiting;

public static class HttpContextUserEmailOrClientIpAddressGetter
{
    public static string GetUserEmailOrClientIpAddress(HttpContext httpContext)
    {
        return httpContext.User.Identity?.IsAuthenticated == true
            ? $"{httpContext.User.Identity?.Name}"
            : $"[{httpContext.Connection.RemoteIpAddress}:{httpContext.Connection.RemotePort}]";
    }
}
