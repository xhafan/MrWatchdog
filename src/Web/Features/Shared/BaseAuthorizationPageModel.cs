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
}