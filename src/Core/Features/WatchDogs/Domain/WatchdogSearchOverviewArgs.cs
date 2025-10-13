using MrWatchdog.Core.Infrastructure.Validations;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogSearchOverviewArgs
{
    [NotDefault]
    public required long WatchdogSearchId { get; set; }
    
    public required string? SearchTerm { get; set; }

    [Display(Name = "Receive email notification about new results")]
    public required bool ReceiveNotification { get; set; }
}