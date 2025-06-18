using CoreDdd.Queries;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageSelectedHtmlModel(IQueryExecutor queryExecutor) : BasePageModel
{
    public WatchdogWebPageSelectedHtmlDto WatchdogWebPageSelectedHtmlDto { get; private set; } = null!;
    
    public async Task OnGet(
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogWebPageSelectedHtmlDto =
            await queryExecutor.ExecuteSingleAsync<GetWatchdogWebPageSelectedHtmlQuery, WatchdogWebPageSelectedHtmlDto>(
                new GetWatchdogWebPageSelectedHtmlQuery(watchdogId, watchdogWebPageId));
    }
}