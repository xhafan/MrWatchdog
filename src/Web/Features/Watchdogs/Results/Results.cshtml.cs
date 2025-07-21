using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Results;

public class ResultsModel(
    IQueryExecutor queryExecutor,
#pragma warning disable CS9113 // Parameter is unread.
    ICoreBus bus
#pragma warning restore CS9113 // Parameter is unread.
) : BasePageModel
{
    public WatchdogResultsArgs WatchdogResultsArgs { get; private set; } = null!;
    
    public async Task OnGet(long id)
    {
        WatchdogResultsArgs = await queryExecutor.ExecuteSingleAsync<GetWatchdogResultsArgsQuery, WatchdogResultsArgs>(new GetWatchdogResultsArgsQuery(id));
    }
}