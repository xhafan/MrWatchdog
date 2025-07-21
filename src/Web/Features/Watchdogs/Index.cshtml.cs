using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Queries;

namespace MrWatchdog.Web.Features.Watchdogs;

public class IndexModel(IQueryExecutor queryExecutor) : PageModel
{
    public IEnumerable<GetWatchdogsQueryResult> WatchdogResults { get; private set; } = null!;
    
    public async Task OnGet()
    {
        WatchdogResults = await queryExecutor.ExecuteAsync<GetWatchdogsQuery, GetWatchdogsQueryResult>(new GetWatchdogsQuery());
    }
}