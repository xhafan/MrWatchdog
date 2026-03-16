using MrWatchdog.Core.Infrastructure.Repositories;
using System.Security.Claims;
using CoreWeb.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public static class ClaimsPrincipalExtensions
{
    extension(ClaimsPrincipal claimsPrincipal)
    {
        public async Task<bool> IsSuperAdmin(IUserRepository userRepository)
        {
            if (!claimsPrincipal.IsAuthenticated())
            {
                return false;
            }

            var user = await userRepository.LoadByIdAsync(claimsPrincipal.GetUserId());
            return user.SuperAdmin;
        }
    }
}