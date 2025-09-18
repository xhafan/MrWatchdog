using System.ComponentModel.DataAnnotations;
using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogWebPageArgs
{
    [NotDefault]
    public long WatchdogId { get; set; }
    [NotDefault]
    public long WatchdogWebPageId { get; set; }

    [Url(ErrorMessage = "The {0} field is not a valid fully-qualified http, or https URL.")]
    [Required]
    public string? Url { get; set; }

    [Display(Name = "CSS path selector")]
    public string? Selector { get; set; }
    
    [Display(Name = "Select text instead of HTML")]
    public bool SelectText { get; set; }
    
    [Required]
    public string? Name { get; set; }
}