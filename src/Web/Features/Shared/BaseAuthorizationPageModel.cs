using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Shared;

public abstract class BaseAuthorizationPageModel(IAuthorizationService authorizationService) : BasePageModel
{
    protected async Task<bool> IsAuthorizedAsScraperOwnerOrSuperAdmin(long scraperId)
    {
        var result = await authorizationService.AuthorizeAsync(
            User, 
            scraperId, 
            new ScraperOwnerOrSuperAdminRequirement()
        );
        return result.Succeeded;
    }

    protected async Task<bool> IsAuthorizedAsWatchdogOwnerOrSuperAdmin(long watchdogId)
    {
        var result = await authorizationService.AuthorizeAsync(
            User, 
            watchdogId, 
            new WatchdogOwnerOrSuperAdminRequirement()
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