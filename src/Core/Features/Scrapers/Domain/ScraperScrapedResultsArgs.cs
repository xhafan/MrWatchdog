using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public record ScraperScrapedResultsArgs
{
    [NotDefault]
    public required long ScraperId { get; set; }
    public required string ScraperName { get; set; }
    public required string? ScraperDescription { get; set; }
    public required IEnumerable<ScraperWebPageScrapedResultsArgs> WebPages { get; set; }
    public required long UserId { get; set; }
    public required PublicStatus PublicStatus { get; set; }
    public required bool IsArchived { get; set; }
}