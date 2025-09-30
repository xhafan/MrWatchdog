using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Web.Features.Shared;
using MrWatchdog.Web.Infrastructure.Authorizations;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageTurboFrameModel(IAuthorizationService authorizationService) : BasePageModel
{
    [BindProperty(SupportsGet = true)]
    public long WatchdogId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long WatchdogWebPageId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!(await authorizationService.AuthorizeAsync(User, WatchdogId, new WatchdogOwnerOrSuperAdminRequirement())).Succeeded)
        {
            return Forbid();
        }
        
        return Page();
    }
}