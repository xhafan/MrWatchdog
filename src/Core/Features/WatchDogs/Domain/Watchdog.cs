using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class Watchdog : VersionedEntity, IAggregateRoot
{
    private readonly IList<WatchdogWebPage> _webPages = new List<WatchdogWebPage>();

    protected Watchdog() {}

    public Watchdog(string name)
    {
        Name = name;
    }
    
    public virtual string Name { get; protected set; } = null!;
    public virtual IEnumerable<WatchdogWebPage> WebPages => _webPages;

    public virtual WatchdogArgs GetWatchdogArgs()
    {
        return new WatchdogArgs
        {
            Id = Id, 
            Name = Name, 
            WebPageIds = WebPages.Select(x => x.Id).ToList()
        };
    }
}