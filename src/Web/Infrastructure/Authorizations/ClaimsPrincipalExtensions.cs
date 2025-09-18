using CoreUtils;
using MrWatchdog.Core.Features.Account;
using System.Security.Claims;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public static class ClaimsPrincipalExtensions
{
    public static bool IsSuperAdmin(this ClaimsPrincipal user)
    {
        if (!user.IsAuthenticated())
        {
            return false;
        }
        
        return user.FindFirst(CustomClaimTypes.SuperAdmin)?.Value == true.ToString().ToLowerInvariant();
    }
    
    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true;
    }  
    
    public static long GetUserId(this ClaimsPrincipal user)
    {
        if (!user.IsAuthenticated())
        {
            return 0;
        }

        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        Guard.Hope(userId != null, "Cannot retrieve userId");

        return long.Parse(userId);
    }       
}