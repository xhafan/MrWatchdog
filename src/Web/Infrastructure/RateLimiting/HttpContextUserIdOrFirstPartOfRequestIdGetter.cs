using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Infrastructure.RateLimiting;

public static class HttpContextUserIdOrFirstPartOfRequestIdGetter
{
    public static string GetUserIdOrFirstPartOfRequestId(HttpContext httpContext)
    {
        if (httpContext.User.IsAuthenticated())
        {
            return $"{httpContext.User.GetUserId()}";
        }

        var firstPartOfRequestId = httpContext.TraceIdentifier.Split(':')[0];
        return firstPartOfRequestId;
    }
}
