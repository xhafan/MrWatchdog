using CoreUtils;
using Microsoft.AspNetCore.Authentication.Cookies;
using MrWatchdog.Core.Infrastructure.Repositories;
using System.Security.Claims;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public static class ClaimsPrincipalExtensions
{
    public static async Task<bool> IsSuperAdmin(this ClaimsPrincipal claimsPrincipal, IUserRepository userRepository)
    {
        if (!claimsPrincipal.IsAuthenticated())
        {
            return false;
        }

        var user = await userRepository.LoadByIdAsync(claimsPrincipal.GetUserId());
        return user.SuperAdmin;
    }
    
    public static bool IsAuthenticated(this ClaimsPrincipal claimsPrincipal)
    {
        return claimsPrincipal.Identity?.IsAuthenticated == true
            && claimsPrincipal.Identity?.AuthenticationType == CookieAuthenticationDefaults.AuthenticationScheme;
    }  
    
    public static long GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        if (!claimsPrincipal.IsAuthenticated())
        {
            return 0;
        }

        var userId = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        Guard.Hope(userId != null, "Cannot retrieve userId");

        return long.Parse(userId);
    }       
}