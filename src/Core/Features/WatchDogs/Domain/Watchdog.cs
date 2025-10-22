using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils;
using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogRequestedToBeMadePublic;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class Watchdog : VersionedEntity, IAggregateRoot
{
    public const int DefaultScrapingIntervalOneDayInSeconds = 86400; // 1 day
    public const int DefaultIntervalBetweenSameResultNotificationsInDays = 30;
    public const int DefaultNumberOfFailedScrapingAttemptsBeforeAlerting = 5;
    
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
        IntervalBetweenSameResultNotificationsInDays = DefaultIntervalBetweenSameResultNotificationsInDays;
        PublicStatus = PublicStatus.Private;
        NumberOfFailedScrapingAttemptsBeforeAlerting = DefaultNumberOfFailedScrapingAttemptsBeforeAlerting;
    }
    
    public virtual User User { get; protected set; } = null!;
    public virtual string Name { get; protected set; } = null!;
    public virtual int ScrapingIntervalInSeconds { get; protected set; }
    public virtual DateTime? NextScrapingOn { get; protected set; }
    public virtual IEnumerable<WatchdogWebPage> WebPages => _webPages;
    public virtual PublicStatus PublicStatus { get; protected set; }
    public virtual double IntervalBetweenSameResultNotificationsInDays { get; protected set; }
    public virtual bool CanNotifyAboutFailedScraping { get; protected set; }
    public virtual int NumberOfFailedScrapingAttemptsBeforeAlerting { get; protected set; }
    public virtual bool IsArchived { get; protected set; }
    
    
    public virtual WatchdogDetailArgs GetWatchdogDetailArgs()
    {
        return new WatchdogDetailArgs
        {
            WatchdogId = Id, 
            WebPageIds = WebPages.Select(x => x.Id).ToList(),
            Name = Name,
            PublicStatus = PublicStatus,
            IsArchived = IsArchived,

            UserId = User.Id,
            UserEmail = User.Email
        };
    }
    
    public virtual WatchdogOverviewArgs GetWatchdogOverviewArgs()
    {
        return new WatchdogOverviewArgs
        {
            WatchdogId = Id, 
            Name = Name,
            ScrapingIntervalInSeconds = ScrapingIntervalInSeconds,
            IntervalBetweenSameResultNotificationsInDays = IntervalBetweenSameResultNotificationsInDays,
            NumberOfFailedScrapingAttemptsBeforeAlerting = NumberOfFailedScrapingAttemptsBeforeAlerting
        };
    }

    public virtual void UpdateOverview(WatchdogOverviewArgs watchdogOverviewArgs)
    {
        Guard.Hope(Id == watchdogOverviewArgs.WatchdogId, "Invalid watchdog Id.");
        
        Name = watchdogOverviewArgs.Name;

        NextScrapingOn = NextScrapingOn?
            .AddSeconds(-ScrapingIntervalInSeconds + watchdogOverviewArgs.ScrapingIntervalInSeconds);

        ScrapingIntervalInSeconds = watchdogOverviewArgs.ScrapingIntervalInSeconds;
        IntervalBetweenSameResultNotificationsInDays = watchdogOverviewArgs.IntervalBetweenSameResultNotificationsInDays;
        NumberOfFailedScrapingAttemptsBeforeAlerting = watchdogOverviewArgs.NumberOfFailedScrapingAttemptsBeforeAlerting;
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

    public virtual WatchdogWebPageDisabledWarningDto GetWatchdogWebPageDisabledWarningDto(long watchdogWebPageId)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        return webPage.GetWatchdogWebPageDisabledWarningArgs();
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
        webPage.SetScrapingResults(scrapingResults, canRaiseScrapingFailedDomainEvent: false);
    }
    
    public virtual void SetScrapingErrorMessage(long watchdogWebPageId, string scrapingErrorMessage)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        webPage.SetScrapingErrorMessage(scrapingErrorMessage, canRaiseScrapingFailedDomainEvent: false);
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
                .Where(x => x.IsEnabled)
                .Select(x => x.GetWatchdogWebPageScrapingResultsArgs())
                .WhereNotNull()
                .ToList(),
            UserId = User.Id,
            PublicStatus = PublicStatus,
            IsArchived = IsArchived
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

        if (!CanNotifyAboutFailedScraping)
        {
            CanNotifyAboutFailedScraping = webPagesToScrape.All(x => x.ScrapingErrorMessage == null);
        }
        
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
        var webPage = _GetWebPage(watchdogWebPageId);
        await webPage.Scrape(httpClientFactory, canRaiseScrapingFailedDomainEvent);
    }

    public virtual void RequestToMakePublic()
    {
        Guard.Hope(PublicStatus != PublicStatus.Public, "Watchdog is already public.");

        PublicStatus = PublicStatus.MakePublicRequested;

        DomainEvents.RaiseEvent(new WatchdogRequestedToBeMadePublicDomainEvent(Id));
    }

    public virtual void MakePublic()
    {
        PublicStatus = PublicStatus.Public;
    }

    public virtual void MakePrivate()
    {
        PublicStatus = PublicStatus.Private;
    }
    
    public virtual WatchdogDetailPublicStatusArgs GetWatchdogDetailPublicStatusArgs()
    {
        return new WatchdogDetailPublicStatusArgs
        {
            WatchdogId = Id, 
            PublicStatus = PublicStatus
        };
    }

    public virtual void EnableWebPage(long watchdogWebPageId)
    {
        var webPage = _GetWebPage(watchdogWebPageId);
        webPage.Enable();
    }

    public virtual async Task NotifyAdminAboutWatchdogRequestedToBeMadePublic(
        IEmailSender emailSender,
        RuntimeOptions runtimeOptions,
        EmailAddressesOptions emailAddressesOptions
    )
    {
        var mrWatchdogResource = Resource.MrWatchdog;
        await emailSender.SendEmail(
            emailAddressesOptions.Admin,
            $"{mrWatchdogResource}: watchdog {Name} requested to be made public",
            $"""
             <p>
                 Watchdog <a href="{runtimeOptions.Url}{WatchdogUrlConstants.WatchdogDetailUrlTemplate.WithWatchdogId(Id)}">{Name}</a> has been requested to be made public by user {User.Email}.
             </p>
             <p>
                 Kind regards,<br>
                 {mrWatchdogResource}
             </p>
             """
        );
    }

    public virtual void DisableNotifyingAboutFailedScraping()
    {
        CanNotifyAboutFailedScraping = false;
    }

    public virtual void Archive()
    {
        IsArchived = true;

        DomainEvents.RaiseEvent(new WatchdogArchivedDomainEvent(Id));
    }
}
