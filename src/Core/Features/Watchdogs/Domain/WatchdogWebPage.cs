using CoreDdd.Domain.Events;
using CoreUtils.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using Ganss.Xss;
using HtmlAgilityPack;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingDataUpdated;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogWebPageScrapingFailed;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Extensions;
using Serilog;
using System.Web;
using CoreUtils;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogWebPage : VersionedEntity
{
    private readonly IList<string> _scrapingResults = new List<string>();
    
    protected WatchdogWebPage() {}

    public WatchdogWebPage(
        Watchdog watchdog,
        WatchdogWebPageArgs args
    )
    {
        Watchdog = watchdog;
        Url = args.Url;
        Selector = args.Selector;
        SelectText = args.SelectText;
        Name = args.Name;
    }
    
    public virtual Watchdog Watchdog { get; } = null!;
    public virtual string? Url { get; protected set; }
    public virtual string? Selector { get; protected set; }
    public virtual bool SelectText { get; protected set; }
    public virtual IEnumerable<string> ScrapingResults => _scrapingResults;
    public virtual string? Name { get; protected set; }
    public virtual DateTime? ScrapedOn { get; protected set; }
    public virtual string? ScrapingErrorMessage { get; protected set; }
    public virtual bool IsEnabled { get; protected set; }

    public virtual WatchdogWebPageArgs GetWatchdogWebPageArgs()
    {
        return new WatchdogWebPageArgs
        {
            WatchdogId = Watchdog.Id,
            WatchdogWebPageId = Id,
            Url = Url, 
            Selector = Selector,
            SelectText = SelectText,
            Name = Name
        };
    }

    public virtual WatchdogWebPageDisabledWarningDto GetWatchdogWebPageDisabledWarningArgs()
    {
        return new WatchdogWebPageDisabledWarningDto
        {
            IsEnabled = IsEnabled,
            HasBeenScrapedSuccessfully = ScrapedOn.HasValue
        };
    }

    public virtual void Update(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        var hasScrapingDataUpdated =
            Url != watchdogWebPageArgs.Url
            || Selector != watchdogWebPageArgs.Selector
            || SelectText != watchdogWebPageArgs.SelectText;

        Url = watchdogWebPageArgs.Url?.Trim();
        Selector = watchdogWebPageArgs.Selector?.Trim();
        SelectText = watchdogWebPageArgs.SelectText;
        Name = watchdogWebPageArgs.Name?.Trim();

        if (!hasScrapingDataUpdated) return;
        
        _ResetScrapingData();
        _Disable();
        
        DomainEvents.RaiseEvent(new WatchdogWebPageScrapingDataUpdatedDomainEvent(Watchdog.Id, Id));
    }

    private void _ResetScrapingData()
    {
        _scrapingResults.Clear();
        ScrapedOn = null;
        ScrapingErrorMessage = null;
    }

    public virtual void SetScrapingResults(
        ICollection<string> scrapingResults,
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        _ResetScrapingData();

        var sanitizer = new HtmlSanitizer();

        if (SelectText)
        {
            scrapingResults = scrapingResults
                .Select(_getTextFromHtml)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            if (scrapingResults.IsEmpty())
            {
                SetScrapingErrorMessage(
                    "All selected HTML results have empty text.",
                    canRaiseScrapingFailedDomainEvent
                );
                return;
            }
        }
        else
        {
            var scrapingResultsWithNonEmptyText = scrapingResults
                .Select(html => sanitizer.Sanitize(html))
                .Where(html => !string.IsNullOrWhiteSpace(_getTextFromHtml(html)))
                .ToList();
            
            if (scrapingResultsWithNonEmptyText.IsEmpty())
            {
                SetScrapingErrorMessage(
                    "All selected HTML results have empty text.",
                    canRaiseScrapingFailedDomainEvent
                );
                return;
            }            
            
            scrapingResults = scrapingResultsWithNonEmptyText;
        }
        
        _scrapingResults.AddRange(scrapingResults);
        ScrapedOn = DateTime.UtcNow;
        return;

        string _getTextFromHtml(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            var textNodes = htmlDoc.DocumentNode
                .DescendantsAndSelf()
                .Where(x => x.NodeType == HtmlNodeType.Text)
                .Select(x => x.InnerText.Trim())
                .Where(x => !string.IsNullOrEmpty(x));
            var joinedText = string.Join(" ", textNodes);
            return HttpUtility.HtmlDecode(joinedText);
        }
    }
    
    public virtual void SetScrapingErrorMessage(
        string scrapingErrorMessage, 
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        _ResetScrapingData();
        
        ScrapingErrorMessage = scrapingErrorMessage;

        if (canRaiseScrapingFailedDomainEvent && Watchdog.CanNotifyAboutFailedScraping)
        {
            DomainEvents.RaiseEvent(new WatchdogWebPageScrapingFailedDomainEvent(Watchdog.Id));
        }

        Log.Information("Setting watchdog web page scraping error message: {scrapingErrorMessage}", scrapingErrorMessage);
    }
    
    public virtual WatchdogWebPageScrapingResultsDto GetWatchdogWebPageScrapingResultsDto()
    {
        return new WatchdogWebPageScrapingResultsDto(Watchdog.Id, Id, _scrapingResults, ScrapedOn, ScrapingErrorMessage);
    }

    public virtual WatchdogWebPageScrapingResultsArgs? GetWatchdogWebPageScrapingResultsArgs()
    {
        return !string.IsNullOrWhiteSpace(Url) 
               && !string.IsNullOrWhiteSpace(Name)
               && string.IsNullOrWhiteSpace(ScrapingErrorMessage)
               && ScrapedOn != null
            ? new WatchdogWebPageScrapingResultsArgs
            {
                Name = Name,
                ScrapingResults = _scrapingResults.ToList(),
                Url = Url
            }
            : null;
    }
    
    public virtual async Task Scrape(
        IHttpClientFactory httpClientFactory,
        bool canRaiseScrapingFailedDomainEvent
        )
    {
        var httpClient = httpClientFactory.CreateClient(HttpClientConstants.HttpClientWithRetries);
        
        string? responseContent;
        
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Url);
            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                SetScrapingErrorMessage(
                    $"Error scraping web page, HTTP status code: {(int) response.StatusCode} {response.ReasonPhrase}",
                    canRaiseScrapingFailedDomainEvent
                );
                return;
            }
            
            responseContent = await response.Content.ReadAsStringWithLimitAsync(
                ScrapingConstants.WebPageSizeLimitInMegaBytes,
                $"Web page larger than {ScrapingConstants.WebPageSizeLimitInMegaBytes} MB."
                );
        }
        catch (Exception ex)
        {
            SetScrapingErrorMessage(ex.Message, canRaiseScrapingFailedDomainEvent);
            return;
        }
        
        List<HtmlNode> selectedHtmlNodes;
        try
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(responseContent);

            selectedHtmlNodes = htmlDoc.DocumentNode.QuerySelectorAll(Selector).ToList();
        }
        catch (Exception ex)
        {
            SetScrapingErrorMessage(ex.Message, canRaiseScrapingFailedDomainEvent);
            
            Log.Error(ex, "Watchdog {WatchdogId} web page {WebPageId} scraping error, selector: {Selector}, html: {responseContent}",
                Watchdog.Id, Id, Selector, responseContent);

            return;
        }
        
        if (selectedHtmlNodes.IsEmpty())
        {
            SetScrapingErrorMessage("No HTML node(s) selected. Please review the Selector.", canRaiseScrapingFailedDomainEvent);
            return;
        }

        SetScrapingResults(
            selectedHtmlNodes.Select(x => x.OuterHtml).ToList(),
            canRaiseScrapingFailedDomainEvent
        );
    }

    public virtual void Enable()
    {
        Guard.Hope(ScrapedOn.HasValue, "Watchdog web page can be enabled only after successful scraping.");

        IsEnabled = true;
    }

    private void _Disable()
    {
        IsEnabled = false;
    }

}