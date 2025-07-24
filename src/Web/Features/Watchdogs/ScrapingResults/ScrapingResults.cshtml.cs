using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

public class ScrapingResultsModel(
    IQueryExecutor queryExecutor,
#pragma warning disable CS9113 // Parameter is unread.
    ICoreBus bus
#pragma warning restore CS9113 // Parameter is unread.
) : BasePageModel
{
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;
    
    [BindProperty]
    public string? SearchTerm { get; set; }
    
    public async Task OnGet(long watchdogId)
    {
        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(watchdogId)
            );
    }
    
    public async Task<IActionResult> OnPostCreateWatchdogAlert(
        long watchdogId, 
        [FromForm] string? searchTerm
    )
    {
        var command = new CreateWatchdogAlertCommand(watchdogId, searchTerm?.Trim());
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}