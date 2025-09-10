using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageScrapingResultsModel(IQueryExecutor queryExecutor) : BasePageModel
{
    public WatchdogWebPageScrapingResultsDto WatchdogWebPageScrapingResultsDto { get; private set; } = null!;
    
    public async Task OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogWebPageScrapingResultsDto =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageScrapingResultsQuery, WatchdogWebPageScrapingResultsDto>(
                new GetWatchdogWebPageScrapingResultsQuery(watchdogId, watchdogWebPageId));
    }
}