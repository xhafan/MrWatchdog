namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record WatchdogSearchArgs
{
    public required long WatchdogSearchId { get; set; }
    public required long ScraperId { get; set; }
    public required string? SearchTerm { get; set; }
    public required PublicStatus ScraperPublicStatus { get; set; }
    public required bool IsArchived { get; set; }
}