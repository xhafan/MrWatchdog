using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogDetailArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }
    public required IList<long> WebPageIds { get; set; } = null!;
    public required string Name { get; set; } = null!;
    public required PublicStatus PublicStatus { get; set; }
}
