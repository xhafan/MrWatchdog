using System.Globalization;
using CoreDdd.Domain.Events;
using CoreUtils;
using CoreUtils.Extensions;
using Fizzler.Systems.HtmlAgilityPack;
using Ganss.Xss;
using HtmlAgilityPack;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingDataUpdated;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.ScraperWebPageScrapingFailed;
using MrWatchdog.Core.Features.Scrapers.Services;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Infrastructure.Localization;
using Serilog;
using System.Web;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class ScraperWebPage : VersionedEntity
{
    private readonly IList<string> _scrapedResults = new List<string>();
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
    public virtual bool ScrapeHtmlAsRenderedByBrowser { get; protected set; }
    public virtual ScrapingByBrowserWaitFor? ScrapingByBrowserWaitFor { get; protected set; }
    public virtual bool SelectText { get; protected set; }
    public virtual IEnumerable<string> ScrapedResults => _scrapedResults;
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
            ScrapeHtmlAsRenderedByBrowser = ScrapeHtmlAsRenderedByBrowser,
            ScrapingByBrowserWaitFor = ScrapingByBrowserWaitFor,
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
        
        var hasDataAffectingScrapingUpdated =
            Url != scraperWebPageArgs.Url
            || Selector != scraperWebPageArgs.Selector
            || ScrapeHtmlAsRenderedByBrowser != scraperWebPageArgs.ScrapeHtmlAsRenderedByBrowser
            || ScrapingByBrowserWaitFor != scraperWebPageArgs.ScrapingByBrowserWaitFor
            || SelectText != scraperWebPageArgs.SelectText
            || !_httpHeaders.AreEquivalent(httpHeaders);

        Url = scraperWebPageArgs.Url?.Trim();
        Selector = scraperWebPageArgs.Selector?.Trim();
        ScrapeHtmlAsRenderedByBrowser = scraperWebPageArgs.ScrapeHtmlAsRenderedByBrowser;
        ScrapingByBrowserWaitFor = scraperWebPageArgs.ScrapingByBrowserWaitFor;
        SelectText = scraperWebPageArgs.SelectText;
        Name = scraperWebPageArgs.Name?.Trim();
        
        _httpHeaders.Clear();
        _httpHeaders.UnionWith(httpHeaders);
        
        if (!raiseScraperWebPageScrapingDataUpdatedDomainEvent) return;

        if (!hasDataAffectingScrapingUpdated) return;
        
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
        _scrapedResults.Clear();
        ScrapedOn = null;
        ScrapingErrorMessage = null;
    }

    public virtual void SetScrapedResults(
        ICollection<string> scrapedResults,
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        _ResetScrapingData();

        var sanitizer = new HtmlSanitizer();

        if (SelectText)
        {
            scrapedResults = scrapedResults
                .Select(_getTextFromHtml)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            if (scrapedResults.IsEmpty())
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
            var scrapedResultsWithNonEmptyText = scrapedResults
                .Select(html => sanitizer.Sanitize(html))
                .Where(html => !string.IsNullOrWhiteSpace(_getTextFromHtml(html)))
                .ToList();
            
            if (scrapedResultsWithNonEmptyText.IsEmpty())
            {
                SetScrapingErrorMessage(
                    "All selected HTML results have empty text.",
                    canRaiseScrapingFailedDomainEvent
                );
                return;
            }            
            
            scrapedResults = scrapedResultsWithNonEmptyText;
        }
        
        _scrapedResults.AddRange(scrapedResults);
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
    
    public virtual ScraperWebPageScrapedResultsDto GetScraperWebPageScrapedResultsDto()
    {
        return new ScraperWebPageScrapedResultsDto(
            Scraper.Id, Id,
            _scrapedResults,
            ScrapedOn,
            ScrapingErrorMessage,
            IsEmptyWebPage: string.IsNullOrWhiteSpace(Url)
        );
    }

    public virtual ScraperWebPageScrapedResultsArgs? GetScraperWebPageScrapedResultsArgs(CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(Url) 
               && !string.IsNullOrWhiteSpace(Name)
               && string.IsNullOrWhiteSpace(ScrapingErrorMessage)
               && ScrapedOn != null
            ? new ScraperWebPageScrapedResultsArgs
            {
                Name = LocalizedTextResolver.ResolveLocalizedText(Name, culture),
                ScrapedResults = _scrapedResults.ToList(),
                Url = Url
            }
            : null;
    }
    
    public virtual async Task Scrape(
        IWebScraperChain webScraperChain,
        bool canRaiseScrapingFailedDomainEvent
    )
    {
        Guard.Hope(!string.IsNullOrWhiteSpace(Url), "Url is not set.");

        var scrapeResult = await webScraperChain.Scrape(
            Url,
            ScrapeHtmlAsRenderedByBrowser, _httpHeaders.Select(x => (x.Name, x.Value)).ToList(),
            new ScrapeOptions {ScrapingByBrowserWaitFor = ScrapingByBrowserWaitFor}
        );
        if (!scrapeResult.Success)
        {
            Guard.Hope(scrapeResult.FailureReason != null, "Scrape result FailureReason is null.");

            SetScrapingErrorMessage(
                scrapeResult.FailureReason,
                canRaiseScrapingFailedDomainEvent
            );
            return;
        }

		var responseContent = scrapeResult.Content;
        Guard.Hope(responseContent != null, "Response content is null.");
        
        responseContent = _ProcessAbsolutePathLinks(responseContent);

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

        SetScrapedResults(
            selectedHtmlNodes.Select(x => x.OuterHtml).ToList(),
            canRaiseScrapingFailedDomainEvent
        );
    }

    public virtual void Enable()
    {
        Guard.Hope(ScrapedOn.HasValue, "Scraper web page can be enabled only after successful scraping.");

        IsEnabled = true;
    }

	private string _ProcessAbsolutePathLinks(string htmlContent) // Does not handle relative path links, e.g. <a href="relative/path">Relative path</a>
    {
        if (string.IsNullOrWhiteSpace(htmlContent) || string.IsNullOrWhiteSpace(Url))
            return htmlContent;

        var uri = new Uri(Url);
        var baseUrl  = $"{uri.Scheme}://{uri.Host}";
        
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htmlContent);

        var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href] | //img[@src]");
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (linkNodes == null) return htmlContent;

        foreach (var node in linkNodes)
        {
            var attributeName = node.Name == "a" ? "href" : "src";
            var attribute = node.Attributes[attributeName];
            var attributeValue = attribute.Value;
            
            // Only convert URLs that start with "/" but not "//" (protocol-relative)
            if (attributeValue.StartsWith("/") && !attributeValue.StartsWith("//"))
            {
                attribute.Value = baseUrl + attributeValue;
            }
        }
            
        return htmlDoc.DocumentNode.OuterHtml;
    }

    private void _Disable()
    {
        IsEnabled = false;
    }
    
    public virtual string? GetLocalizedName(CultureInfo culture)
    {
        return LocalizedTextResolver.ResolveLocalizedText(Name, culture);
    }
}