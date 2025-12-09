using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Detail.Overview;

public class OverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty]
    public ScraperOverviewArgs ScraperOverviewArgs { get; set; } = null!;

    public async Task<IActionResult> OnGet(long scraperId)
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(scraperId)) return Forbid();

        ScraperOverviewArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperOverviewArgsQuery, ScraperOverviewArgs>(
                new GetScraperOverviewArgsQuery(scraperId));

        return Page();
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!await IsAuthorizedAsScraperOwnerOrSuperAdmin(ScraperOverviewArgs.ScraperId)) return Forbid();

        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateScraperOverviewCommand(ScraperOverviewArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}