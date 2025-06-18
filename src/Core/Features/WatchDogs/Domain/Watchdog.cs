using CoreDdd.Domain;
using CoreUtils;
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
            WatchdogId = Id, 
            WebPageIds = WebPages.Select(x => x.Id).ToList()
        };
    }
    
    public virtual WatchdogOverviewArgs GetWatchdogOverviewArgs()
    {
        return new WatchdogOverviewArgs
        {
            WatchdogId = Id, 
            Name = Name
        };
    }

    public virtual void UpdateOverview(WatchdogOverviewArgs watchdogOverviewArgs)
    {
        Guard.Hope(Id == watchdogOverviewArgs.WatchdogId, "Invalid watchdog Id.");
        
        Name = watchdogOverviewArgs.Name;
    }

    public virtual void AddWebPage(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        _webPages.Add(new WatchdogWebPage(
            this,
            watchdogWebPageArgs.Url,
            watchdogWebPageArgs.Selector,
            watchdogWebPageArgs.Name
        ));
    }
    
    public virtual WatchdogWebPageArgs GetWatchdogWebPageArgs(long watchdogWebPageId)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        return webPage.GetWatchdogWebPageArgs();
    }

    private WatchdogWebPage _GetWebPage(long watchdogWebPageId)
    {
        return _webPages.Single(x => x.Id == watchdogWebPageId);
    }

    public virtual void UpdateWebPage(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        var webPage = _GetWebPage(watchdogWebPageArgs.WatchdogWebPageId);
        webPage.Update(watchdogWebPageArgs);
    }

    public virtual void RemoveWebPage(long watchdogWebPageId)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        _webPages.Remove(webPage);
    }

    public virtual void SetSelectedHtml(long watchdogWebPageId, string selectedHtml)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        webPage.SetSelectedHtml(selectedHtml);
    }
    
    public virtual WatchdogWebPageSelectedHtmlDto GetWatchdogWebPageSelectedHtmlDto(long watchdogWebPageId)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        return webPage.GetWatchdogWebPageSelectedHtmlDto();
    }    
}
