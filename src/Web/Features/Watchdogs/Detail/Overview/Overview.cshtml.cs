using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.Overview;

public class OverviewModel(IQueryExecutor queryExecutor) : BasePageModel
{
    public WatchdogArgs WatchdogArgs { get; set; } = null!;

    public async Task OnGet(long id) // todo: test me
    {
        WatchdogArgs = (
            await queryExecutor.ExecuteAsync<GetWatchdogArgsQuery, WatchdogArgs>(new GetWatchdogArgsQuery(id))
        ).Single();
    }
    
    public void OnPost(long id)
    {
    }
}