using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogResultsArgs
{
    [NotDefault]
    public required long WatchdogId { get; set; }
    public required string Name { get; set; } = null!;
}