using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScraperDetailArgs
{
    [NotDefault]
    public required long ScraperId { get; set; }
    public required IList<long> WebPageIds { get; set; }
    public required string Name { get; set; }
    public required PublicStatus PublicStatus { get; set; }
    public required bool IsArchived { get; set; }

    public required long UserId { get; set; }
    public required string UserEmail { get; set; }
}
