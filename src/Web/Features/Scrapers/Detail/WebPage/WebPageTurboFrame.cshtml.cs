using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageTurboFrameModel(IAuthorizationService authorizationService) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty(SupportsGet = true)]
    public long WatchdogId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long WatchdogWebPageId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(WatchdogId)) return Forbid();
       
        return Page();
    }
}