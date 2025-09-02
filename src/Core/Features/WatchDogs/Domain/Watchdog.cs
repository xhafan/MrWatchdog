using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils;
using CoreUtils.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapingCompleted;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class Watchdog : VersionedEntity, IAggregateRoot
{
    private const int ScrapingIntervalOneDayInSeconds = 86400;
    
    private readonly IList<WatchdogWebPage> _webPages = new List<WatchdogWebPage>();

    protected Watchdog() {}

    public Watchdog(
        User user, 
        string name
    )
    {
        User = user;
        Name = name;
        ScrapingIntervalInSeconds = ScrapingIntervalOneDayInSeconds;
    }
    
    public virtual User User { get; protected set; } = null!;
    public virtual string Name { get; protected set; } = null!;
    public virtual int ScrapingIntervalInSeconds { get; protected set; }
    public virtual DateTime? NextScrapingOn { get; protected set; }
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
            Name = Name,
            ScrapingIntervalInSeconds = ScrapingIntervalInSeconds
        };
    }

    public virtual void UpdateOverview(WatchdogOverviewArgs watchdogOverviewArgs)
    {
        Guard.Hope(Id == watchdogOverviewArgs.WatchdogId, "Invalid watchdog Id.");
        
        Name = watchdogOverviewArgs.Name;

        NextScrapingOn = NextScrapingOn?
            .AddSeconds(-ScrapingIntervalInSeconds + watchdogOverviewArgs.ScrapingIntervalInSeconds);

        ScrapingIntervalInSeconds = watchdogOverviewArgs.ScrapingIntervalInSeconds;
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

    public virtual async Task Scrape(IHttpClientFactory httpClientFactory)
    {
        var webPagesToScrape = _webPages.Where(x => !string.IsNullOrWhiteSpace(x.Url)).ToList();

        foreach (var webPage in webPagesToScrape)
        {
            await ScrapeWebPage(webPage.Id, httpClientFactory);
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

    public virtual async Task ScrapeWebPage(long watchdogWebPageId, IHttpClientFactory httpClientFactory)
    {
        var webPage = _GetWebPage(watchdogWebPageId);

        var httpClient = httpClientFactory.CreateClient();
        
        string? responseContent = null;
        string? scrapingErrorMessage = null;
        
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, webPage.Url);
            var response = await httpClient.SendAsync(request);
            responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                scrapingErrorMessage = $"Error scraping web page, HTTP status code: {(int) response.StatusCode} {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            scrapingErrorMessage = ex.Message;
        }

        if (scrapingErrorMessage != null)
        {
            webPage.SetScrapingErrorMessage(scrapingErrorMessage);
            return;
        }
        
        Guard.Hope(responseContent != null, nameof(responseContent) + " is null");

        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(responseContent);
        
        var nodes = htmlDoc.DocumentNode.QuerySelectorAll(webPage.Selector).ToList();
        if (nodes.IsEmpty())
        {
            scrapingErrorMessage = "No HTML node selected. Please review the Selector.";
        } 

        if (scrapingErrorMessage != null)
        {
            webPage.SetScrapingErrorMessage(scrapingErrorMessage);
            return;
        }        
        
        webPage.SetScrapingResults(nodes.Select(x => x.OuterHtml).ToList());
    }
}
