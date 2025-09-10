using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Infrastructure;

namespace MrWatchdog.Web.Features.Logs;

// todo: prevent abuse - make it harder to log error - see recommendations: https://chatgpt.com/share/68c16f0c-9bf8-8000-8846-80ef03c7b267
[ApiController]
[Route("api/[controller]/[action]")]
public class LogsController(ILogger<LogsController> logger) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(LogConstants.MaxLogMessageLength + 500)] // 500 bytes buffer
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