using System.Web;
using CoreDdd.Domain.Events;
using CoreUtils;
using CoreUtils.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using Ganss.Xss;
using HtmlAgilityPack;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Extensions;
using Serilog;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScraperWebPage : VersionedEntity
{
    private readonly IList<string> _scrapingResults = new List<string>();
    private readonly ISet<ScraperWebPageHttpHeader> _httpHeaders = new HashSet<ScraperWebPageHttpHeader>();

    protected ScraperWebPage() {}

    public ScraperWebPage(
        Scraper scraper,
        ScraperWebPageArgs args
    )
    {
        Scraper = scraper;

        Update(args, raiseScraperWebPageScrapingDataUpdatedDomainEvent: false);
    }
    
    public virtual Scraper Scraper { get; } = null!;
    public virtual string? Url { get; protected set; }
    public virtual string? Selector { get; protected set; }
    public virtual bool SelectText { get; protected set; }
    public virtual IEnumerable<string> ScrapingResults => _scrapingResults;
    public virtual string? Name { get; protected set; }
    public virtual DateTime? ScrapedOn { get; protected set; }
    public virtual string? ScrapingErrorMessage { get; protected set; }
    public virtual bool IsEnabled { get; protected set; }
    public virtual int NumberOfFailedScrapingAttemptsBeforeTheNextAlert { get; protected set; }
    public virtual IEnumerable<ScraperWebPageHttpHeader> HttpHeaders => _httpHeaders;

    public virtual ScraperWebPageArgs GetScraperWebPageArgs()
    {
        return new ScraperWebPageArgs
        {
            ScraperId = Scraper.Id,
            ScraperWebPageId = Id,
            Url = Url, 
            Selector = Selector,
            SelectText = SelectText,
            Name = Name,
            HttpHeaders = string.Join(Environment.NewLine, _httpHeaders.Select(httpHeader => $"{httpHeader.Name}: {httpHeader.Value}"))
        };
    }

    public virtual ScraperWebPageDisabledWarningDto GetScraperWebPageDisabledWarningArgs()
    {
        return new ScraperWebPageDisabledWarningDto
        {
            IsEnabled = IsEnabled,
            HasBeenScrapedSuccessfully = ScrapedOn.HasValue
        };
    }

    public virtual void Update(
        ScraperWebPageArgs scraperWebPageArgs, 
        bool raiseScraperWebPageScrapingDataUpdatedDomainEvent = true
    )
    {
        var httpHeaders = _getHttpHeaders().ToList();
        
        var hasScrapingDataUpdated =
            Url != scraperWebPageArgs.Url
            || Selector != scraperWebPageArgs.Selector
            || SelectText != scraperWebPageArgs.SelectText
            || !_httpHeaders.AreEquivalent(httpHeaders);

        Url = scraperWebPageArgs.Url?.Trim();
        Selector = scraperWebPageArgs.Selector?.Trim();
        SelectText = scraperWebPageArgs.SelectText;
        Name = scraperWebPageArgs.Name?.Trim();
        
        _httpHeaders.Clear();
        _httpHeaders.UnionWith(httpHeaders);

        if (!raiseScraperWebPageScrapingDataUpdatedDomainEvent) return;

        if (!hasScrapingDataUpdated) return;
        
        _ResetScrapingData();
        _Disable();
        
        DomainEvents.RaiseEvent(new ScraperWebPageScrapingDataUpdatedDomainEvent(Scraper.Id, Id));
        return;

        IEnumerable<ScraperWebPageHttpHeader> _getHttpHeaders()
        {
            if (string.IsNullOrWhiteSpace(scraperWebPageArgs.HttpHeaders)) yield break;

            var lines = scraperWebPageArgs.HttpHeaders.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var line in lines)
            {
                var parts = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                Guard.Hope(parts.Length == 2, $"""
                                               Invalid header format: "{line}". Expected format is "header name: value".
                                               """);
                yield return new ScraperWebPageHttpHeader(parts[0], parts[1]);
            }
        }
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
        ResetNumberOfFailedScrapingAttemptsBeforeTheNextAlert();
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

    private void ResetNumberOfFailedScrapingAttemptsBeforeTheNextAlert()
    {
        NumberOfFailedScrapingAttemptsBeforeTheNextAlert = 0;
    }

    public virtual void SetScrapingErrorMessage(
        string scrapingErrorMessage, 
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        _ResetScrapingData();
        
        ScrapingErrorMessage = scrapingErrorMessage;

        if (canRaiseScrapingFailedDomainEvent && Scraper.CanNotifyAboutFailedScraping)
        {
            NumberOfFailedScrapingAttemptsBeforeTheNextAlert++;
            
            if (NumberOfFailedScrapingAttemptsBeforeTheNextAlert >= Scraper.NumberOfFailedScrapingAttemptsBeforeAlerting)
            {
                DomainEvents.RaiseEvent(new ScraperWebPageScrapingFailedDomainEvent(Scraper.Id));
                
                Scraper.DisableNotifyingAboutFailedScraping();
                ResetNumberOfFailedScrapingAttemptsBeforeTheNextAlert();
            }
        }

        Log.Information("Setting scraper web page scraping error message: {scrapingErrorMessage}", scrapingErrorMessage);
    }
    
    public virtual ScraperWebPageScrapingResultsDto GetScraperWebPageScrapingResultsDto()
    {
        return new ScraperWebPageScrapingResultsDto(Scraper.Id, Id, _scrapingResults, ScrapedOn, ScrapingErrorMessage);
    }

    public virtual ScraperWebPageScrapingResultsArgs? GetScraperWebPageScrapingResultsArgs()
    {
        return !string.IsNullOrWhiteSpace(Url) 
               && !string.IsNullOrWhiteSpace(Name)
               && string.IsNullOrWhiteSpace(ScrapingErrorMessage)
               && ScrapedOn != null
            ? new ScraperWebPageScrapingResultsArgs
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
            foreach (var httpHeader in _httpHeaders)
            {
                request.Headers.Add(httpHeader.Name, httpHeader.Value);
            }
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
            
            Log.Error(ex, "Scraper {ScraperId} web page {WebPageId} scraping error, selector: {Selector}, html: {responseContent}",
                Scraper.Id, Id, Selector, responseContent);

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
        Guard.Hope(ScrapedOn.HasValue, "Scraper web page can be enabled only after successful scraping.");

        IsEnabled = true;
    }

    private void _Disable()
    {
        IsEnabled = false;
    }

}