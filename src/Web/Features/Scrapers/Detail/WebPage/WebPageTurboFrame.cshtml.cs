using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.WebPage;

public class WebPageTurboFrameModel(IAuthorizationService authorizationService) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty(SupportsGet = true)]
    public long ScraperId { get; set; }

    [BindProperty(SupportsGet = true)]
    public long ScraperWebPageId { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(ScraperId)) return Forbid();
       
        return Page();
    }
}