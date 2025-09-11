using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;

namespace MrWatchdog.Web.Features.Watchdogs.ScrapingResults;

/// This class exist due to Razor pages (ScrapingResultsModel) not able to mix anonymous and authenticated requests
[ApiController]
[Route("api/[controller]/[action]")]
public class ScrapingResultsController(ICoreBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateWatchdogAlert(
        long watchdogId, 
        [FromForm] string? searchTerm
    )
    {
        var command = new CreateWatchdogAlertCommand(watchdogId, searchTerm?.Trim());
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}