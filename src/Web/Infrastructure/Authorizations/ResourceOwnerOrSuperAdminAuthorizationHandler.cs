using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class ResourceOwnerOrSuperAdminAuthorizationHandler(IUserRepository userRepository) 
    : AuthorizationHandler<ResourceOwnerOrSuperAdminRequirement, long>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerOrSuperAdminRequirement requirement,
        long resourceUserId
    )
    {
        if (context.User.GetUserId() == resourceUserId
            || await context.User.IsSuperAdmin(userRepository))
        {
            context.Succeed(requirement);
        }
    }
}