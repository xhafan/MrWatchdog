using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Watchdogs.Create;

public class CreateModel(ICoreBus bus) : BasePageModel
{
    [BindProperty]
    [Required]
    public string Name { get; set; } = null!;
    
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        var command = new CreateWatchdogCommand(Name);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}