using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Badges;

public class BadgesModel(IQueryExecutor queryExecutor) : BasePageModel
{
    [BindProperty]
    public WatchdogDetailPublicStatusArgs WatchdogDetailPublicStatusArgs { get; set; } = null!;
    
    [BindProperty(SupportsGet = true)]
    public bool ShowPrivate { get; set; } = true;

    public async Task OnGet(long watchdogId)
    {
        WatchdogDetailPublicStatusArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogDetailPublicStatusArgsQuery, WatchdogDetailPublicStatusArgs>(
                new GetWatchdogDetailPublicStatusArgsQuery(watchdogId));
    }
}