using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

[Authorize]
public class WebPageTurboFrameModel : BasePageModel
{
    public long WatchdogId { get; private set; }
    public long WatchdogWebPageId { get; private set; }

    public void OnGet(long watchdogId, long watchdogWebPageId)
    {
        WatchdogId = watchdogId;
        WatchdogWebPageId = watchdogWebPageId;
    }
}