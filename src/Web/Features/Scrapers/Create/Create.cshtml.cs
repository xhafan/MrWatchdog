using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features;
using MrWatchdog.Core.Features.Scrapers.Commands;
using MrWatchdog.Core.Infrastructure.ActingUserAccessors;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Resources;
using MrWatchdog.Web.Features.Shared;

namespace MrWatchdog.Web.Features.Scrapers.Create;

public class CreateModel(ICoreBus bus, IActingUserAccessor actingUserAccessor) : BasePageModel
{
    [BindProperty]
    [Required]
    [StringLength(ValidationConstants.ScraperNameMaxLength)]
    [Display(Name = nameof(Resource.Name), ResourceType = typeof(Resource))]
    public string Name { get; set; } = null!;
    
    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        var command = new CreateScraperCommand(actingUserAccessor.GetActingUserId(), Name);
        await bus.Send(command);
        return Ok(command.Guid.ToString());
    }    
}