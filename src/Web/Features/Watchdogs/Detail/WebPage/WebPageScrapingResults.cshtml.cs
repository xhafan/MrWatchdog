using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageScrapingResultsModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    [BindProperty(SupportsGet = true)]
    public long WatchdogId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public long WatchdogWebPageId { get; set; }

    public WatchdogWebPageScrapingResultsDto WatchdogWebPageScrapingResults { get; private set; } = null!;
    
    public async Task<IActionResult> OnGet()
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(WatchdogId)) return Forbid();

        WatchdogWebPageScrapingResults =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageScrapingResultsQuery, WatchdogWebPageScrapingResultsDto>(
                new GetWatchdogWebPageScrapingResultsQuery(WatchdogId, WatchdogWebPageId));
        
        return Page();
    }

    public async Task<IActionResult> OnPostScrapeWatchdogWebPage()
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(WatchdogId)) return Forbid();

        var command = new ScrapeWatchdogWebPageCommand(WatchdogId, WatchdogWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}