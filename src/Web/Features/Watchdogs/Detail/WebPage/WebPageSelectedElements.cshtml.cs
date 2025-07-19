using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageSelectedElementsModel(IQueryExecutor queryExecutor) : BasePageModel
{
    public WatchdogWebPageSelectedElementsDto WatchdogWebPageSelectedElementsDto { get; private set; } = null!;
    
    public async Task OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogWebPageSelectedElementsDto =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageSelectedElementsQuery, WatchdogWebPageSelectedElementsDto>(
                new GetWatchdogWebPageSelectedElementsQuery(watchdogId, watchdogWebPageId));
    }
}