using CoreDdd.Domain.Events;
using CoreUtils;
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
    private readonly IList<string> _selectedElements = new List<string>();
    
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
    public virtual IEnumerable<string> SelectedElements => _selectedElements;
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
        _selectedElements.Clear();
        ScrapedOn = null;
        ScrapingErrorMessage = null;
    }

    public virtual void SetSelectedElements(ICollection<string> selectedElements)
    {
        _ResetScrapingData();

        if (SelectText)
        {
            selectedElements = selectedElements
                .Select(html =>
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
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            if (selectedElements.IsEmpty())
            {
                SetScrapingErrorMessage("No text inside selected HTML.");
                return;
            }
        }
        else
        {
            Guard.Hope(selectedElements.Any(x => !string.IsNullOrWhiteSpace(x)), "All selected elements are empty.");

            var sanitizer = new HtmlSanitizer();
            selectedElements = selectedElements.Select(x => sanitizer.Sanitize(x)).ToList();
        }
        
        _selectedElements.AddRange(selectedElements);
        ScrapedOn = DateTime.UtcNow;
    }
    
    public virtual void SetScrapingErrorMessage(string scrapingErrorMessage)
    {
        _ResetScrapingData();
        
        ScrapingErrorMessage = scrapingErrorMessage;
    }    
    
    public virtual WatchdogWebPageSelectedElementsDto GetWatchdogWebPageSelectedElementsDto()
    {
        return new WatchdogWebPageSelectedElementsDto(Watchdog.Id, Id, _selectedElements, ScrapedOn, ScrapingErrorMessage);
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
                SelectedElements = _selectedElements.ToList(),
                Url = Url
            }
            : null;
    }
}