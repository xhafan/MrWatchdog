using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Web.Features.Logs;

// todo: prevet abuse
[ApiController]
[Route("api/[controller]/[action]")]
public class LogsController(ILogger<LogsController> logger) : ControllerBase
{
    [HttpPost]
    public IActionResult LogError([FromBody] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return BadRequest("Message cannot be empty.");
        }

        var safeMessage = message.Length > LogConstants.MaxLogMessageLength 
            ? message.Substring(0, LogConstants.MaxLogMessageLength) + "…[truncated]" 
            : message;

        logger.LogError(safeMessage);

        return Ok();
    }
}