using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class WatchdogOwnerOrSuperAdminAuthorizationHandler(
    IUserRepository userRepository,
    IRepository<Watchdog> watchdogRepository
) 
    : AuthorizationHandler<WatchdogOwnerOrSuperAdminRequirement, long>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WatchdogOwnerOrSuperAdminRequirement requirement,
        long watchdogId
    )
    {
        var watchdog = await watchdogRepository.LoadByIdAsync(watchdogId);
        if (context.User.GetUserId() == watchdog.User.Id
            || await context.User.IsSuperAdmin(userRepository))
        {
            context.Succeed(requirement);
        }
    }
}