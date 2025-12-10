using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MrWatchdog.Core.Features.Scrapers;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Search;

public class SearchModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public WatchdogSearchArgs WatchdogSearchArgs { get; private set; } = null!;
    public ScraperScrapingResultsArgs ScraperScrapingResultsArgs { get; private set; } = null!;

    public async Task<IActionResult> OnGet(long watchdogSearchId)
    {
        WatchdogSearchArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogSearchArgsQuery, WatchdogSearchArgs>(
                new GetWatchdogSearchArgsQuery(watchdogSearchId)
            );
        
        if (!await IsAuthorizedAsWatchdogSearchOwnerOrSuperAdmin(watchdogSearchId))
        {
            if (WatchdogSearchArgs.ScraperPublicStatus != PublicStatus.Public) return Forbid();

            var redirectUrl = QueryHelpers.AddQueryString(
                ScraperUrlConstants.ScraperScrapingResultsUrlTemplate.WithScraperId(WatchdogSearchArgs.ScraperId),
                new Dictionary<string, string?>
                {
                    {"searchTerm", WatchdogSearchArgs.SearchTerm}
                }
            );
            return Redirect(redirectUrl);
        }

        ScraperScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetScraperScrapingResultsArgsQuery, ScraperScrapingResultsArgs>(
                new GetScraperScrapingResultsArgsQuery(WatchdogSearchArgs.ScraperId)
            );
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostArchiveWatchdogSearch(long watchdogSearchId)
    {
        if (!await IsAuthorizedAsWatchdogSearchOwnerOrSuperAdmin(watchdogSearchId)) return Forbid();

        var command = new ArchiveWatchdogSearchCommand(watchdogSearchId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}