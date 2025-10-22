using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogSearchOverviewArgs
{
    [NotDefault]
    public required long WatchdogSearchId { get; set; }
    
    public required string? SearchTerm { get; set; }

    public required bool ReceiveNotification { get; set; }
}