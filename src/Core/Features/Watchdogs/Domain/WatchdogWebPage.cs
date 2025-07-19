using CoreDdd.Domain.Events;
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

    public virtual void SetSelectedElements(IEnumerable<string> selectedElements)
    {
        _ResetScrapingData();

        if (SelectText)
        {
            selectedElements = selectedElements.Select(html =>
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
            });
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
}