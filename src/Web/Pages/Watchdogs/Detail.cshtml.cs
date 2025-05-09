using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Web.Pages.Shared;

namespace MrWatchdog.Web.Pages.Watchdogs;

public class DetailModel : BasePageModel
{
    [BindProperty]
    public WatchdogArgs WatchdogArgs { get; set; } = null!;
    
    public void OnGet(long id) // todo: test me
    {
        var watchdogWebPageArgsList = new List<long> {23};

        WatchdogArgs = new WatchdogArgs
        {
            Id = id,
            Name = "Travelzoo top 20",
            WebPageIds = watchdogWebPageArgsList
        };
    }
    
    public IActionResult OnPost() // todo: test me
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        return Page();
    }    
}