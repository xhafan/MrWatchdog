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

    public string? Selector { get; set; }
    
    public string? Name { get; set; }
}