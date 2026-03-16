using CoreWeb.Features.Shared;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Shared;

public abstract class BaseAuthorizationPageModel(IAuthorizationService authorizationService) : BaseCoreWebAuthorizationPageModel(authorizationService)
{
    private readonly IAuthorizationService _authorizationService = authorizationService;

    protected async Task<bool> IsAuthorizedAsScraperOwnerOrSuperAdmin(long scraperId)
    {
        var result = await _authorizationService.AuthorizeAsync(
            User, 
            scraperId, 
            new ScraperOwnerOrSuperAdminRequirement()
        );
        return result.Succeeded;
    }

    protected async Task<bool> IsAuthorizedAsWatchdogOwnerOrSuperAdmin(long watchdogId)
    {
        var result = await _authorizationService.AuthorizeAsync(
            User, 
            watchdogId, 
            new WatchdogOwnerOrSuperAdminRequirement()
        );
        return result.Succeeded;
    }
}