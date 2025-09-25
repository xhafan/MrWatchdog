using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageDisabledWarningModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus
) : BasePageModel
{
    [BindProperty(SupportsGet = true)]
    public long WatchdogId { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public long WatchdogWebPageId { get; set; }

    public WatchdogWebPageDisabledWarningDto WatchdogWebPageDisabledWarningDto { get; private set; } = null!;

    public async Task OnGet()
    {
        WatchdogWebPageDisabledWarningDto =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageDisabledWarningQuery, WatchdogWebPageDisabledWarningDto>(
                new GetWatchdogWebPageDisabledWarningQuery(WatchdogId, WatchdogWebPageId));
    }
    
    public async Task<IActionResult> OnPostEnableWatchdogWebPage()
    {
        var command = new EnableWatchdogWebPageCommand(WatchdogId, WatchdogWebPageId);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}