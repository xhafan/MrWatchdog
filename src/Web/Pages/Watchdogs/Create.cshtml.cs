using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Web.Pages.Shared;

namespace MrWatchdog.Web.Pages.Watchdogs;

public class CreateModel : BasePageModel
{
    [BindProperty]
    [Required]
    public string Name { get; set; } = null!;
    
    public IActionResult OnPost() // todo: test me
    {
        if (!ModelState.IsValid)
        {
            return PageWithUnprocessableEntityStatus422();
        }

        return RedirectToPage("Detail", new {id = 12});
    }    
}