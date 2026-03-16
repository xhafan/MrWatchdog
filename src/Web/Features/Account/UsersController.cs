using CoreBackend.Infrastructure.Rebus;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Commands;

namespace MrWatchdog.Web.Features.Account;

[ApiController]
[Route("api/[controller]/[action]")]
public class UsersController(ICoreBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CompleteOnboarding(string onboardingIdentifier)
    {
        var command = new CompleteOnboardingCommand(onboardingIdentifier);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }
}