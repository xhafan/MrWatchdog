using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public record WatchdogScrapingResultsArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }
    public required string WatchdogName { get; set; }
    public required IEnumerable<WatchdogWebPageScrapingResultsArgs> WebPages { get; set; }
}