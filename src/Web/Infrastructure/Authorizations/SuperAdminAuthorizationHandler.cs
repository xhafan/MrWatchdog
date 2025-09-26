using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class SuperAdminAuthorizationHandler(IUserRepository userRepository) : AuthorizationHandler<SuperAdminRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, SuperAdminRequirement requirement)
    {
        var userId = context.User.GetUserId();
        if (userId == 0) return;

        var user = await userRepository.LoadByIdAsync(userId);
        if (user.SuperAdmin)
        {
            context.Succeed(requirement);
        }
    }
}