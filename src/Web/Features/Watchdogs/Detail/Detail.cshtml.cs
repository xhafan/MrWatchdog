using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

public class DetailModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public WatchdogArgs WatchdogArgs { get; private set; } = null!;
    public ScraperScrapedResultsArgs ScraperScrapedResultsArgs { get; private set; } = null!;

    public async Task<IActionResult> OnGet(long watchdogId)
    {
        WatchdogArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogArgsQuery, WatchdogArgs>(
                new GetWatchdogArgsQuery(watchdogId)
            );
        
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId))
        {
            if (WatchdogArgs.ScraperPublicStatus != PublicStatus.Public) return Forbid();

            var redirectUrl = QueryHelpers.AddQueryString(
                ScraperUrlConstants.ScraperScrapedResultsUrlTemplate.WithScraperId(WatchdogArgs.ScraperId),
                new Dictionary<string, string?>
                {
                    {"searchTerm", WatchdogArgs.SearchTerm}
                }
            );
            return Redirect(redirectUrl);
        }

        ScraperScrapedResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperScrapedResultsArgsQuery, ScraperScrapedResultsArgs>(
                new GetScraperScrapedResultsArgsQuery(WatchdogArgs.ScraperId)
            );
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostArchiveWatchdog(long watchdogId)
    {
        if (!await IsAuthorizedAsWatchdogOwnerOrSuperAdmin(watchdogId)) return Forbid();

        var command = new ArchiveWatchdogCommand(watchdogId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}