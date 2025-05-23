using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.Web.Features.Shared;
using Rebus.Bus;

namespace MrWatchdog.Web.Features.Watchdogs;

public class CreateModel(IBus bus) : BasePageModel
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