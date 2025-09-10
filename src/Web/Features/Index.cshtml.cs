using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MrWatchdog.Web.Features;

[AllowAnonymous]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}