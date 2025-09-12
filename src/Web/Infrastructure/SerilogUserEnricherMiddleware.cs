using MrWatchdog.Core.Infrastructure;
using System.Security.Claims;

namespace MrWatchdog.Web.Infrastructure;

public class SerilogUserEnricherMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var userId = httpContext.User.Identity?.IsAuthenticated == true
            ? long.Parse(httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!)
            : 0;

        using (Serilog.Context.LogContext.PushProperty(LogConstants.UserId, $"{userId}"))
        {
            await next(httpContext);
        }
    }
}