using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageOverviewModel(
    IQueryExecutor queryExecutor, 
    ICoreBus bus
) : BasePageModel
{
    [BindProperty]
    public WatchdogWebPageArgs WatchdogWebPageArgs { get; set; } = null!;
    public bool IsEmptyWebPage { get; private set; }

    public async Task OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogWebPageArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageArgsQuery, WatchdogWebPageArgs>(
                new GetWatchdogWebPageArgsQuery(watchdogId, watchdogWebPageId));

        IsEmptyWebPage = string.IsNullOrWhiteSpace(WatchdogWebPageArgs.Url);
    }
    
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }
        
        var command = new UpdateWatchdogWebPageCommand(WatchdogWebPageArgs);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}