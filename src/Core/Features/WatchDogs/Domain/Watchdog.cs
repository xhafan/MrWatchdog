using CoreDdd.Domain;
using CoreUtils;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;

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

    public virtual WatchdogDetailArgs GetWatchdogDetailArgs()
    {
        return new WatchdogDetailArgs
        {
            WatchdogId = Id, 
            WebPageIds = WebPages.Select(x => x.Id).ToList(),
            Name = Name
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
            watchdogWebPageArgs
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

    public virtual void SetScrapingResults(long watchdogWebPageId, ICollection<string> scrapingResults)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        webPage.SetScrapingResults(scrapingResults);
    }
    
    public virtual void SetScrapingErrorMessage(long watchdogWebPageId, string scrapingErrorMessage)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        webPage.SetScrapingErrorMessage(scrapingErrorMessage);
    }    
    
    public virtual WatchdogWebPageScrapingResultsDto GetWatchdogWebPageScrapingResultsDto(long watchdogWebPageId)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        return webPage.GetWatchdogWebPageScrapingResultsDto();
    }
    
    public virtual WatchdogScrapingResultsArgs GetWatchdogScrapingResultsArgs()
    {
        return new WatchdogScrapingResultsArgs
        {
            WatchdogId = Id,
            WatchdogName = Name,
            WebPages = _webPages
                .Select(x => x.GetWatchdogWebPageScrapingResultsArgs())
                .WhereNotNull()
                .ToList()
        };
    }    
}
