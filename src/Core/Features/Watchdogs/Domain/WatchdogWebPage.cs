using CoreDdd.Domain.Events;
using CoreUtils.Extensions;
using Ganss.Xss;
using HtmlAgilityPack;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events;
using MrWatchdog.Core.Infrastructure.Extensions;
using System.Web;

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

    public virtual void Update(WatchdogWebPageArgs watchdogWebPageArgs)
    {
        var hasScrapingDataUpdated =
            Url != watchdogWebPageArgs.Url
            || Selector != watchdogWebPageArgs.Selector
            || SelectText != watchdogWebPageArgs.SelectText;

        Url = watchdogWebPageArgs.Url;
        Selector = watchdogWebPageArgs.Selector;
        SelectText = watchdogWebPageArgs.SelectText;
        Name = watchdogWebPageArgs.Name;

        if (!hasScrapingDataUpdated) return;
        
        _ResetScrapingData();

        DomainEvents.RaiseEvent(new WatchdogWebPageScrapingDataUpdatedDomainEvent(Watchdog.Id, Id));
    }

    private void _ResetScrapingData()
    {
        _scrapingResults.Clear();
        ScrapedOn = null;
        ScrapingErrorMessage = null;
    }

    public virtual void SetScrapingResults(ICollection<string> scrapingResults)
    {
        _ResetScrapingData();

        var sanitizer = new HtmlSanitizer();

        if (SelectText)
        {
            scrapingResults = scrapingResults
                .Select(html => _getTextFromHtml(sanitizer.Sanitize(html)))
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList();

            if (scrapingResults.IsEmpty())
            {
                SetScrapingErrorMessage("All selected HTML results have empty text.");
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
                SetScrapingErrorMessage("All selected HTML results have empty text.");
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
    
    public virtual void SetScrapingErrorMessage(string scrapingErrorMessage)
    {
        _ResetScrapingData();
        
        ScrapingErrorMessage = scrapingErrorMessage;
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
}