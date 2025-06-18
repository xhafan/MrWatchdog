using CoreDdd.Queries;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

public class WebPageSelectedHtmlModel(
#pragma warning disable CS9113 // Parameter is unread.
    IQueryExecutor queryExecutor
#pragma warning restore CS9113 // Parameter is unread.
) : BasePageModel
{
    public long WatchdogWebPageId { get; private set; }
    
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task OnGet( // todo: test me
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        long watchdogId, 
        long watchdogWebPageId
    )
    {
        WatchdogWebPageId = watchdogWebPageId;
    }
}