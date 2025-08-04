using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _previousScrapedResults = new List<string>(); // todo: store previous scraped results
    private readonly IList<string> _currentScrapedResults = new List<string>();
    
    protected WatchdogAlert() {}

    public WatchdogAlert(
        Watchdog watchdog,
        string? searchTerm
    )
    {
        Watchdog = watchdog;
        SearchTerm = searchTerm;

        var scrapedResults = watchdog.WebPages.SelectMany(x => x.SelectedElements);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            scrapedResults = scrapedResults.Where(x => x.Contains(searchTerm)); // todo: extract text from HTML and implement diacritics-insensitive and case-insensitive search
        }
        _currentScrapedResults.AddRange(scrapedResults);
    }
    
    public virtual Watchdog Watchdog { get; } = null!;
    public virtual string? SearchTerm { get; protected set; }
    public virtual IEnumerable<string> PreviousScrapedResults => _previousScrapedResults;
    public virtual IEnumerable<string> CurrentScrapedResults => _currentScrapedResults;

    public virtual WatchdogAlertArgs GetWatchdogAlertArgs()
    {
        return new WatchdogAlertArgs
        {
            WatchdogAlertId = Id,
            WatchdogId = Watchdog.Id,
            SearchTerm = SearchTerm
        };
    }

    public virtual WatchdogAlertOverviewArgs GetWatchdogAlertOverviewArgs()
    {
        return new WatchdogAlertOverviewArgs
        {
            WatchdogAlertId = Id,
            SearchTerm = SearchTerm
        };
    }

    public virtual void UpdateOverview(WatchdogAlertOverviewArgs args)
    {
        SearchTerm = args.SearchTerm;
    }
}