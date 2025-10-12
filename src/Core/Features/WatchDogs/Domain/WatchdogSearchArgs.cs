namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogSearchArgs
{
    public required long WatchdogSearchId { get; set; }
    public required long WatchdogId { get; set; }
    public required string? SearchTerm { get; set; }
    public required PublicStatus WatchdogPublicStatus { get; set; }
}