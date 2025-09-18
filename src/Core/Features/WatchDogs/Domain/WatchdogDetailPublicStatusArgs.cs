using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogDetailPublicStatusArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }
    public required PublicStatus PublicStatus { get; set; }
}