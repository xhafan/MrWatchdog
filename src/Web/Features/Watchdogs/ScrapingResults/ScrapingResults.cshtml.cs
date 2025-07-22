using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

public class ScrapingResultsModel(
    IQueryExecutor queryExecutor,
#pragma warning disable CS9113 // Parameter is unread.
    ICoreBus bus
#pragma warning restore CS9113 // Parameter is unread.
) : BasePageModel
{
    public WatchdogScrapingResultsArgs WatchdogScrapingResultsArgs { get; private set; } = null!;
    
    public async Task OnGet(long id)
    {
        WatchdogScrapingResultsArgs =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogScrapingResultsArgsQuery, WatchdogScrapingResultsArgs>(
                new GetWatchdogScrapingResultsArgsQuery(id)
            );
    }
}