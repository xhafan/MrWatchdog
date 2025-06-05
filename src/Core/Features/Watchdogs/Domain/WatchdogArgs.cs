using MrWatchdog.Core.Infrastructure.Validations;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogArgs
{
    [NotDefault]
    public long WatchdogId { get; set; }

    public IList<long> WebPageIds { get; set; } = new List<long>();
}
