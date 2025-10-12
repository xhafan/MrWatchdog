using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Search;

public class SearchModel(
    IQueryExecutor queryExecutor,
    ICoreBus bus,
    IAuthorizationService authorizationService
) : BaseAuthorizationPageModel(authorizationService)
{
    public WatchdogSearchArgs WatchdogSearchArgs { get; private set; } = null!;
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;

    [BindProperty] 
    public string? SearchTerm { get; set; }

    public async Task<IActionResult> OnGet(long watchdogSearchId)
    {
        WatchdogSearchArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogSearchArgsQuery, WatchdogSearchArgs>(
                new GetWatchdogSearchArgsQuery(watchdogSearchId)
            );
        
        if (!await IsAuthorizedAsWatchdogSearchOwnerOrSuperAdmin(watchdogSearchId))
        {
            if (WatchdogSearchArgs.WatchdogPublicStatus != PublicStatus.Public) return Forbid();

            var redirectUrl = QueryHelpers.AddQueryString(
                WatchdogUrlConstants.WatchdogScrapingResultsUrlTemplate.WithWatchdogId(WatchdogSearchArgs.WatchdogId),
                new Dictionary<string, string?>
                {
                    {"searchTerm", WatchdogSearchArgs.SearchTerm}
                }
            );
            return Redirect(redirectUrl);
        }

        SearchTerm = WatchdogSearchArgs.SearchTerm;

        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(WatchdogSearchArgs.WatchdogId)
            );
        
        return Page();
    }
    
    public async Task<IActionResult> OnPostDeleteWatchdogSearch(long watchdogSearchId)
    {
        if (!await IsAuthorizedAsWatchdogSearchOwnerOrSuperAdmin(watchdogSearchId)) return Forbid();

        var command = new DeleteWatchdogSearchCommand(watchdogSearchId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}