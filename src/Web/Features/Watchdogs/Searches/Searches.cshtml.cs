using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Searches;

public class SearchesModel(
    IQueryExecutor queryExecutor, 
    IActingUserAccessor actingUserAccessor
) : BasePageModel
{
    public IEnumerable<GetWatchdogSearchesQueryResult> WatchdogSearches { get; private set; } = null!;
    
    public async Task OnGet()
    {
        WatchdogSearches = await queryExecutor.ExecuteAsync<GetWatchdogSearchesQuery, GetWatchdogSearchesQueryResult>(
            new GetWatchdogSearchesQuery(actingUserAccessor.GetActingUserId())
        );
    }
}