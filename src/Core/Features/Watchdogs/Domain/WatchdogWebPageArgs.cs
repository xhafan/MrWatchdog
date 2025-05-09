using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWebPageArgs
{
    public long Id { get; set; }
    
    public string? Name { get; set; }

    [Url(ErrorMessage = "The {0} field is not a valid fully-qualified http, or https URL.")]
    public string Url { get; set; } = null!;

    public string? Selector { get; set; } = null!;
}