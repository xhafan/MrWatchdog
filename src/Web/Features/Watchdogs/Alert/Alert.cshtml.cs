using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Alert;

public class AlertModel(
    IQueryExecutor queryExecutor
) : BasePageModel
{
    public WatchdogAlertArgs WatchdogAlertArgs { get; private set; } = null!;
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;

    [BindProperty] 
    public string? SearchTerm { get; set; }

    public async Task OnGet(long watchdogAlertId)
    {
        WatchdogAlertArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogAlertArgsQuery, WatchdogAlertArgs>(
                new GetWatchdogAlertArgsQuery(watchdogAlertId)
            );
        
        SearchTerm = WatchdogAlertArgs.SearchTerm;

        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(WatchdogAlertArgs.WatchdogId)
            );       
    }
}