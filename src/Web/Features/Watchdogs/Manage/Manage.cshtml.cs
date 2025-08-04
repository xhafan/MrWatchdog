using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Manage;

public class ManageModel(IQueryExecutor queryExecutor) : BasePageModel
{
    public IEnumerable<GetWatchdogsQueryResult> WatchdogResults { get; private set; } = null!;
    
    public async Task OnGet()
    {
        WatchdogResults = await queryExecutor.ExecuteAsync<GetWatchdogsQuery, GetWatchdogsQueryResult>(new GetWatchdogsQuery());
    }
}