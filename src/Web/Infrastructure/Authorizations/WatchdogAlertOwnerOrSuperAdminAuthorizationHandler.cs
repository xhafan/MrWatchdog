using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Web.Infrastructure.Authorizations;

public class WatchdogAlertOwnerOrSuperAdminAuthorizationHandler(
    IUserRepository userRepository,
    IRepository<WatchdogAlert> watchdogAlertRepository
) 
    : AuthorizationHandler<WatchdogAlertOwnerOrSuperAdminRequirement, long>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        WatchdogAlertOwnerOrSuperAdminRequirement requirement,
        long watchdogAlertId
    )
    {
        var watchdogAlert = await watchdogAlertRepository.LoadByIdAsync(watchdogAlertId);
        if (context.User.GetUserId() == watchdogAlert.User.Id
            || await context.User.IsSuperAdmin(userRepository))
        {
            context.Succeed(requirement);
        }
    }
}