using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScraperDetailPublicStatusArgs
{
    [NotDefault]
    public required long ScraperId { get; set; }
    public required PublicStatus PublicStatus { get; set; }
}