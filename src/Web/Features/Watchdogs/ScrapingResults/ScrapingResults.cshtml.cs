using CoreDdd.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

[AllowAnonymous]
public class ScrapingResultsModel(
    IQueryExecutor queryExecutor
) : BasePageModel
{
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }
    
    public async Task OnGet(long watchdogId)
    {
        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(watchdogId)
            );
    }
}