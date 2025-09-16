using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils;
using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class Watchdog : VersionedEntity, IAggregateRoot
{
    public const int DefaultScrapingIntervalOneDayInSeconds = 86400; // 1 day
    public const int DefaultIntervalBetweenSameResultAlertsInDays = 30;
    
    private readonly IList<WatchdogWebPage> _webPages = new List<WatchdogWebPage>();

    protected Watchdog() {}

    public Watchdog(
        User user, 
        string name
    )
    {
        User = user;
        Name = name;
        ScrapingIntervalInSeconds = DefaultScrapingIntervalOneDayInSeconds;
        IntervalBetweenSameResultAlertsInDays = DefaultIntervalBetweenSameResultAlertsInDays;
    }
    
    public virtual User User { get; protected set; } = null!;
    public virtual string Name { get; protected set; } = null!;
    public virtual int ScrapingIntervalInSeconds { get; protected set; }
    public virtual DateTime? NextScrapingOn { get; protected set; }
    public virtual IEnumerable<WatchdogWebPage> WebPages => _webPages;
    public virtual bool MakePublicRequested { get; protected set; }
    public virtual bool Public { get; protected set; }
    public virtual double IntervalBetweenSameResultAlertsInDays { get; protected set; }
    public virtual bool CanNotifyAboutFailedScraping { get; protected set; }
    

    public virtual WatchdogDetailArgs GetWatchdogDetailArgs()
    {
        return new WatchdogDetailArgs
        {
            WatchdogId = Id, 
            WebPageIds = WebPages.Select(x => x.Id).ToList(),
            Name = Name,
            MakePublicRequested = MakePublicRequested,
            Public = Public
        };
    }
    
    public virtual WatchdogOverviewArgs GetWatchdogOverviewArgs()
    {
        return new WatchdogOverviewArgs
        {
            WatchdogId = Id, 
            Name = Name,
            ScrapingIntervalInSeconds = ScrapingIntervalInSeconds,
            IntervalBetweenSameResultAlertsInDays = IntervalBetweenSameResultAlertsInDays
        };
    }

    public virtual void UpdateOverview(WatchdogOverviewArgs watchdogOverviewArgs)
    {
        Guard.Hope(Id == watchdogOverviewArgs.WatchdogId, "Invalid watchdog Id.");
        
        Name = watchdogOverviewArgs.Name;

        NextScrapingOn = NextScrapingOn?
            .AddSeconds(-ScrapingIntervalInSeconds + watchdogOverviewArgs.ScrapingIntervalInSeconds);

        ScrapingIntervalInSeconds = watchdogOverviewArgs.ScrapingIntervalInSeconds;
        IntervalBetweenSameResultAlertsInDays = watchdogOverviewArgs.IntervalBetweenSameResultAlertsInDays;
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
        var webPage = GetWebPage(watchdogWebPageId);
        return webPage.GetWatchdogWebPageArgs();
    }

    public virtual WatchdogWebPage GetWebPage(long watchdogWebPageId)
    {
        return _webPages.Single(x => x.Id == watchdogWebPageId);
    }

    public virtual void UpdateWebPage(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        var webPage = GetWebPage(watchdogWebPageArgs.WatchdogWebPageId);
        webPage.Update(watchdogWebPageArgs);
    }

    public virtual void RemoveWebPage(long watchdogWebPageId)
    {
        var webPage = GetWebPage(watchdogWebPageId);
        _webPages.Remove(webPage);
    }

    public virtual void SetScrapingResults(long watchdogWebPageId, ICollection<string> scrapingResults)
    {
        var webPage = GetWebPage(watchdogWebPageId);
        webPage.SetScrapingResults(scrapingResults, canRaiseScrapingFailedDomainEvent: false);
    }
    
    public virtual void SetScrapingErrorMessage(long watchdogWebPageId, string scrapingErrorMessage)
    {
        var webPage = GetWebPage(watchdogWebPageId);
        webPage.SetScrapingErrorMessage(scrapingErrorMessage, canRaiseScrapingFailedDomainEvent: false);
    }    
    
    public virtual WatchdogWebPageScrapingResultsDto GetWatchdogWebPageScrapingResultsDto(long watchdogWebPageId)
    {
        var webPage = GetWebPage(watchdogWebPageId);
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

    public virtual async Task Scrape(IHttpClientFactory httpClientFactory)
    {
        var webPagesToScrape = _webPages.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
        
        if (webPagesToScrape.IsEmpty()) return;

        foreach (var webPage in webPagesToScrape)
        {
            await ScrapeWebPage(
                webPage.Id,
                httpClientFactory,
                canRaiseScrapingFailedDomainEvent: true
            );
        }

        CanNotifyAboutFailedScraping = webPagesToScrape.All(x => x.ScrapingErrorMessage == null);
        
        DomainEvents.RaiseEvent(new WatchdogScrapingCompletedDomainEvent(Id));
    }
    
    public virtual void ScheduleNextScraping()
    {
        var utcNow = DateTime.UtcNow;
        NextScrapingOn ??= utcNow;
        NextScrapingOn = NextScrapingOn.Value.AddSeconds(ScrapingIntervalInSeconds);
        if (NextScrapingOn.Value < utcNow)
        {
            NextScrapingOn = utcNow.AddSeconds(ScrapingIntervalInSeconds);
        }
    }

    public virtual void SetNextScrapingOn(DateTime? nextScrapingOn)
    {
        NextScrapingOn = nextScrapingOn;
    }

    public virtual async Task ScrapeWebPage(
        long watchdogWebPageId, 
        IHttpClientFactory httpClientFactory,
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        var webPage = GetWebPage(watchdogWebPageId);
        await webPage.ScrapeWebPage(httpClientFactory, canRaiseScrapingFailedDomainEvent);
    }

    public virtual void RequestToMakePublic()
    {
        Guard.Hope(!Public, "Watchdog is already public."); // todo: make this UserException shown to the user

        MakePublicRequested = true;
    }

    public virtual void MakePublic()
    {
        Public = true;
        MakePublicRequested = false;
    }

    public virtual void MakePrivate()
    {
        Public = false;
        MakePublicRequested = false;
    }
    
    public virtual WatchdogDetailPublicStatusArgs GetWatchdogDetailPublicStatusArgs()
    {
        return new WatchdogDetailPublicStatusArgs
        {
            WatchdogId = Id, 
            MakePublicRequested = MakePublicRequested,
            Public = Public
        };
    }
}
