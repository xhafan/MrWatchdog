using Microsoft.AspNetCore.Http.Features;

namespace MrWatchdog.Web.Infrastructure.Middlewares;

public class ForwardedPortMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext httpContext)
    {
        var forwardedPort = httpContext.Request.Headers["X-Forwarded-Port"].FirstOrDefault();

        if (forwardedPort != null && int.TryParse(forwardedPort, out var port))
        {
            var originalFeature = httpContext.Features.Get<IHttpConnectionFeature>();
            originalFeature?.RemotePort = port;
        }

        await next(httpContext);
    }
}
