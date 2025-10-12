using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class WatchdogSearchOwnerOrSuperAdminAuthorizationHandler(
    IUserRepository userRepository,
    IRepository<WatchdogSearch> watchdogSearchRepository
) 
    : AuthorizationHandler<WatchdogSearchOwnerOrSuperAdminRequirement, long>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WatchdogSearchOwnerOrSuperAdminRequirement requirement,
        long watchdogSearchId
    )
    {
        var watchdogSearch = await watchdogSearchRepository.LoadByIdAsync(watchdogSearchId);
        if (context.User.GetUserId() == watchdogSearch.User.Id
            || await context.User.IsSuperAdmin(userRepository))
        {
            context.Succeed(requirement);
        }
    }
}