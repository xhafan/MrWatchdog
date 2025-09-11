using System.Security.Claims;
using MrWatchdog.Core.Features.Account;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public static class ClaimsPrincipalExtensions
{
    public static bool IsSuperAdmin(this ClaimsPrincipal user)
    {
        return user.FindFirst(CustomClaimTypes.SuperAdmin)?.Value == true.ToString().ToLowerInvariant();
    }
    
    public static bool IsAuthenticated(this ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true;
    }    
}