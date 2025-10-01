using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Shared;

public abstract class BaseAuthorizationPageModel(IAuthorizationService authorizationService) : BasePageModel
{
    protected async Task<bool> IsAuthorizedAsWatchdogOwnerOrSuperAdmin(long watchdogId)
    {
        var result = await authorizationService.AuthorizeAsync(
            User, 
            watchdogId, 
            new WatchdogOwnerOrSuperAdminRequirement()
        );
        return result.Succeeded;
    }

    protected async Task<bool> IsAuthorizedAsWatchdogAlertOwnerOrSuperAdmin(long watchdogAlertId)
    {
        var result = await authorizationService.AuthorizeAsync(
            User, 
            watchdogAlertId, 
            new WatchdogAlertOwnerOrSuperAdminRequirement()
        );
        return result.Succeeded;
    }

    protected async Task<bool> IsAuthorizedAsSuperAdmin()
    {
        var result = await authorizationService.AuthorizeAsync(
            User, 
            null, 
            new SuperAdminRequirement()
        );
        return result.Succeeded;
    }
}