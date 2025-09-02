using CoreDdd.Queries;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MrWatchdog.Core.Features.Watchdogs.Queries;

namespace MrWatchdog.Web.Features.Watchdogs;

public class IndexModel(IQueryExecutor queryExecutor) : PageModel
{
    public IEnumerable<GetWatchdogsQueryResult> Watchdogs { get; private set; } = null!;
    
    public async Task OnGet()
    {
        // todo: add a new query to load public watchdogs only
        Watchdogs = await queryExecutor.ExecuteAsync<GetWatchdogsQuery, GetWatchdogsQueryResult>(new GetWatchdogsQuery(0)); // todo: get actual user id
    }
}