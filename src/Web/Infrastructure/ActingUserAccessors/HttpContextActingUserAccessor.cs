using System.Security.Claims;
using CoreUtils;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;

namespace MrWatchdog.Web.Infrastructure.ActingUserAccessors;

public class HttpContextActingUserAccessor(IHttpContextAccessor httpContextAccessor) : IActingUserAccessor
{
    public long GetActingUserId()
    {
        if (httpContextAccessor.HttpContext == null)
        {
            return 0;
        }
        
        var claimsPrincipal = httpContextAccessor.HttpContext.User;

        if (claimsPrincipal.Identity?.IsAuthenticated == false)
        {
            return 0;
        }
        
        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        Guard.Hope(userId != null, "Cannot retrieve userId");
        
        return long.Parse(userId);
    }
}