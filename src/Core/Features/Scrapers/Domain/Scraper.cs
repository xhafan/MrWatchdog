using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils;
using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperArchived;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperRequestedToBeMadePublic;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperScrapingCompleted;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Resources;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class Scraper : VersionedEntity, IAggregateRoot
{
    public const int DefaultScrapingIntervalOneDayInSeconds = 86400; // 1 day
    public const int DefaultIntervalBetweenSameResultNotificationsInDays = 30;
    public const int DefaultNumberOfFailedScrapingAttemptsBeforeAlerting = 5;
    
    private readonly IList<ScraperWebPage> _webPages = new List<ScraperWebPage>();

    protected Scraper() {}

    public Scraper(
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
    public virtual string? Description { get; protected set; }
    public virtual int ScrapingIntervalInSeconds { get; protected set; }
    public virtual DateTime? NextScrapingOn { get; protected set; }
    public virtual IEnumerable<ScraperWebPage> WebPages => _webPages;
    public virtual PublicStatus PublicStatus { get; protected set; }
    public virtual double IntervalBetweenSameResultNotificationsInDays { get; protected set; }
    public virtual bool CanNotifyAboutFailedScraping { get; protected set; }
    public virtual int NumberOfFailedScrapingAttemptsBeforeAlerting { get; protected set; }
    public virtual bool IsArchived { get; protected set; }
    
    
    public virtual ScraperDetailArgs GetScraperDetailArgs()
    {
        return new ScraperDetailArgs
        {
            ScraperId = Id, 
            WebPageIds = WebPages.Select(x => x.Id).ToList(),
            Name = Name,
            PublicStatus = PublicStatus,
            IsArchived = IsArchived,

            UserId = User.Id,
            UserEmail = User.Email
        };
    }
    
    public virtual ScraperOverviewArgs GetScraperOverviewArgs()
    {
        return new ScraperOverviewArgs
        {
            ScraperId = Id, 
            Name = Name,
            Description = Description,
            ScrapingIntervalInSeconds = ScrapingIntervalInSeconds,
            IntervalBetweenSameResultNotificationsInDays = IntervalBetweenSameResultNotificationsInDays,
            NumberOfFailedScrapingAttemptsBeforeAlerting = NumberOfFailedScrapingAttemptsBeforeAlerting
        };
    }

    public virtual void UpdateOverview(ScraperOverviewArgs scraperOverviewArgs)
    {
        Guard.Hope(Id == scraperOverviewArgs.ScraperId, "Invalid scraper Id.");
        
        Name = scraperOverviewArgs.Name;
        Description = scraperOverviewArgs.Description;

        NextScrapingOn = NextScrapingOn?
            .AddSeconds(-ScrapingIntervalInSeconds + scraperOverviewArgs.ScrapingIntervalInSeconds);

        ScrapingIntervalInSeconds = scraperOverviewArgs.ScrapingIntervalInSeconds;
        IntervalBetweenSameResultNotificationsInDays = scraperOverviewArgs.IntervalBetweenSameResultNotificationsInDays;
        NumberOfFailedScrapingAttemptsBeforeAlerting = scraperOverviewArgs.NumberOfFailedScrapingAttemptsBeforeAlerting;
    }

    public virtual void AddWebPage(ScraperWebPageArgs scraperWebPageArgs)
    {
        _webPages.Add(new ScraperWebPage(
            this,
            scraperWebPageArgs
        ));
    }
    
    public virtual ScraperWebPageArgs GetScraperWebPageArgs(long scraperWebPageId)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        return webPage.GetScraperWebPageArgs();
    }

    public virtual ScraperWebPageDisabledWarningDto GetScraperWebPageDisabledWarningDto(long scraperWebPageId)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        return webPage.GetScraperWebPageDisabledWarningArgs();
    }

    private ScraperWebPage _GetWebPage(long scraperWebPageId)
    {
        return _webPages.Single(x => x.Id == scraperWebPageId);
    }

    public virtual void UpdateWebPage(ScraperWebPageArgs scraperWebPageArgs)
    {
        var webPage = _GetWebPage(scraperWebPageArgs.ScraperWebPageId);
        webPage.Update(scraperWebPageArgs);
    }

    public virtual void RemoveWebPage(long scraperWebPageId)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        _webPages.Remove(webPage);
    }

    public virtual void SetScrapingResults(long scraperWebPageId, ICollection<string> scrapingResults)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        webPage.SetScrapingResults(scrapingResults, canRaiseScrapingFailedDomainEvent: false);
    }
    
    public virtual void SetScrapingErrorMessage(long scraperWebPageId, string scrapingErrorMessage)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        webPage.SetScrapingErrorMessage(scrapingErrorMessage, canRaiseScrapingFailedDomainEvent: false);
    }    
    
    public virtual ScraperWebPageScrapingResultsDto GetScraperWebPageScrapingResultsDto(long scraperWebPageId)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        return webPage.GetScraperWebPageScrapingResultsDto();
    }
    
    public virtual ScraperScrapingResultsArgs GetScraperScrapingResultsArgs()
    {
        return new ScraperScrapingResultsArgs
        {
            ScraperId = Id,
            ScraperName = Name,
            ScraperDescription = Description,
            WebPages = _webPages
                .Where(x => x.IsEnabled)
                .Select(x => x.GetScraperWebPageScrapingResultsArgs())
                .WhereNotNull()
                .ToList(),
            UserId = User.Id,
            PublicStatus = PublicStatus,
            IsArchived = IsArchived
        };
    }

    public virtual async Task Scrape(IWebScraperChain webScraperChain)
    {
        var webPagesToScrape = _webPages.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();
        
        if (webPagesToScrape.IsEmpty()) return;

        foreach (var webPage in webPagesToScrape)
        {
            await ScrapeWebPage(
                webPage.Id,
                webScraperChain,
                canRaiseScrapingFailedDomainEvent: true
            );
        }

        if (!CanNotifyAboutFailedScraping)
        {
            CanNotifyAboutFailedScraping = webPagesToScrape.All(x => x.ScrapingErrorMessage == null);
        }
        
        DomainEvents.RaiseEvent(new ScraperScrapingCompletedDomainEvent(Id));
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
        long scraperWebPageId, 
        IWebScraperChain webScraperChain,
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        var webPage = _GetWebPage(scraperWebPageId);
        await webPage.Scrape(webScraperChain, canRaiseScrapingFailedDomainEvent);
    }

    public virtual void RequestToMakePublic()
    {
        Guard.Hope(PublicStatus != PublicStatus.Public, "Scraper is already public.");

        PublicStatus = PublicStatus.RequestedToBeMadePublic;

        DomainEvents.RaiseEvent(new ScraperRequestedToBeMadePublicDomainEvent(Id));
    }

    public virtual void MakePublic()
    {
        PublicStatus = PublicStatus.Public;
    }

    public virtual void MakePrivate()
    {
        PublicStatus = PublicStatus.Private;
    }
    
    public virtual ScraperDetailPublicStatusArgs GetScraperDetailPublicStatusArgs()
    {
        return new ScraperDetailPublicStatusArgs
        {
            ScraperId = Id, 
            PublicStatus = PublicStatus
        };
    }

    public virtual void EnableWebPage(long scraperWebPageId)
    {
        var webPage = _GetWebPage(scraperWebPageId);
        webPage.Enable();
    }

    public virtual async Task NotifyAdminAboutScraperRequestedToBeMadePublic(
        ICoreBus bus,
        RuntimeOptions runtimeOptions,
        EmailAddressesOptions emailAddressesOptions
    )
    {
        var mrWatchdogResource = Resource.MrWatchdog;
        await bus.Send(new SendEmailCommand(
            emailAddressesOptions.Admin,
            $"{mrWatchdogResource}: scraper {Name} requested to be made public",
            $"""
             <html>
             <body>
             <p>
                 Scraper <a href="{runtimeOptions.Url}{ScraperUrlConstants.ScraperDetailUrlTemplate.WithScraperId(Id)}">{Name}</a> has been requested to be made public by user {User.Email}.
             </p>
             <p>
                 Kind regards,<br>
                 {mrWatchdogResource}
             </p>
             </body>
             </html>
             """
        ));
    }

    public virtual void DisableNotifyingAboutFailedScraping()
    {
        CanNotifyAboutFailedScraping = false;
    }

    public virtual void Archive()
    {
        IsArchived = true;

        DomainEvents.RaiseEvent(new ScraperArchivedDomainEvent(Id));
    }
}
