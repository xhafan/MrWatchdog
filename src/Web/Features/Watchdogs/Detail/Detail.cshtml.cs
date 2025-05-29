using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail;

public class DetailModel(
    IQueryExecutor queryExecutor
) : BasePageModel
{
    [BindProperty]
    public WatchdogArgs WatchdogArgs { get; set; } = null!;
    
    public async Task OnGet(long id)
    {
        WatchdogArgs = (
            await queryExecutor.ExecuteAsync<GetWatchdogArgsQuery, WatchdogArgs>(new GetWatchdogArgsQuery(id))
        ).Single();
    }
}