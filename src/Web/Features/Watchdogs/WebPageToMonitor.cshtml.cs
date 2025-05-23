using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs;

public class WebPageToMonitorModel : BasePageModel
{
    [BindProperty]
    public WatchdogWebPageArgs WatchdogWebPageArgs { get; set; } = null!;

    public void OnGet(long id) // todo: test me
    {
        string? name = null;
        string url = null!;
        
        if (id == 23)
        {
            name = "Travelzoo US";
            url = "https://www.travelzoo.com/top20/";
        }
        
        if (id == 24)
        {
            name = "Travelzoo DE";
            url = "https://www.travelzoo.com/de/top20/";
        }        
        
        WatchdogWebPageArgs = new WatchdogWebPageArgs
        {
            Id = id,
            Name = name,
            Url = url
        };
    }
}