using MrWatchdog.Core.Features.Scrapers.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogArgs
{
    public required long WatchdogId { get; set; }
    public required long ScraperId { get; set; }
    public required string? SearchTerm { get; set; }
    public required PublicStatus ScraperPublicStatus { get; set; }
    public required bool IsArchived { get; set; }
}