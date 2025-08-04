using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogAlertOverviewArgs
{
    [NotDefault]
    public required long WatchdogAlertId { get; set; }
    
    public required string? SearchTerm { get; set; }
}