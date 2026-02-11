using System.Globalization;
using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.Badges;

public class BadgesModel(
    IQueryExecutor queryExecutor,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public ScraperDetailArgs ScraperDetailArgs { get; set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public bool ShowPrivate { get; set; } = true;

    public async Task<IActionResult> OnGet(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        ScraperDetailArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperDetailArgsQuery, ScraperDetailArgs>(
                new GetScraperDetailArgsQuery(scraperId, CultureInfo.CurrentUICulture));

        return Page();
    }
}