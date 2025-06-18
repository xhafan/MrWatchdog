using CoreDdd.Domain.Events;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;

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
    public virtual string? SelectedHtml { get; protected set; }
    public virtual DateTime? ScrapedOn { get; protected set; }

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
        var hasChanged =
            Url != watchdogWebPageArgs.Url
            || Selector != watchdogWebPageArgs.Selector
            || Name != watchdogWebPageArgs.Name;

        Url = watchdogWebPageArgs.Url;
        Selector = watchdogWebPageArgs.Selector;
        Name = watchdogWebPageArgs.Name;

        if (!hasChanged) return;
        
        SelectedHtml = null;
        ScrapedOn = null;
            
        DomainEvents.RaiseEvent(new WatchdogWebPageUpdatedDomainEvent(Watchdog.Id, Id));
    }

    public virtual void SetSelectedHtml(string selectedHtml)
    {
        SelectedHtml = selectedHtml;
        ScrapedOn = DateTime.UtcNow;
    }
    
    public virtual WatchdogWebPageSelectedHtmlDto GetWatchdogWebPageSelectedHtmlDto()
    {
        return new WatchdogWebPageSelectedHtmlDto(Watchdog.Id, Id, SelectedHtml, ScrapedOn);
    }      
}