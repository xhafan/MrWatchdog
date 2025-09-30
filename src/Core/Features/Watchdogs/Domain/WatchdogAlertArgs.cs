namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogAlertArgs
{
    public required long WatchdogAlertId { get; set; }
    public required long WatchdogId { get; set; }
    public required string? SearchTerm { get; set; }
    public required PublicStatus WatchdogPublicStatus { get; set; }
}