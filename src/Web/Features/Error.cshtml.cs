using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : BasePageModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    [BindProperty(SupportsGet = true)]
    public string? ErrorMessage { get; set; }
    
    [BindProperty(SupportsGet = true)]
    public int? HttpStatusCode { get; set; }

    public void OnGet()
    {
        RequestId = HttpContext.TraceIdentifier;
    }
}