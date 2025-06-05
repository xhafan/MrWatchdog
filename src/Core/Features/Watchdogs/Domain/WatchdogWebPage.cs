using MrWatchdog.Core.Features.Shared.Domain;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWebPage : VersionedEntity
{
    protected WatchdogWebPage() {}

    public WatchdogWebPage(
        Watchdog watchdog,
        string? url, 
        string? selector, 
        string? name
    )
    {
        Watchdog = watchdog;
        Url = url;
        Selector = selector;
        Name = name;
    }
    
    public virtual Watchdog Watchdog { get; } = null!;
    public virtual string? Url { get; protected set; }
    public virtual string? Selector { get; protected set; }
    public virtual string? Name { get; protected set; }

    public virtual string GetHtmlSnippet()
    {
        return "";
    }

    public virtual WatchdogWebPageArgs GetWatchdogWebPageArgs()
    {
        return new WatchdogWebPageArgs
        {
            WatchdogId = Watchdog.Id,
            WatchdogWebPageId = Id,
            Url = Url, 
            Selector = Selector,
            Name = Name
        };
    }

    public virtual void Update(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        Url = watchdogWebPageArgs.Url;
        Selector = watchdogWebPageArgs.Selector;
        Name = watchdogWebPageArgs.Name;
    }
}