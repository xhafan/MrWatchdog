namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogArgs
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public IList<long> WebPageIds { get; set; } = new List<long>();
}
