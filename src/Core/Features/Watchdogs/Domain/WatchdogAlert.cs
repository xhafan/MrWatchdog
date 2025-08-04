using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _previousScrapingResults = new List<string>(); // todo: store previous scraping results
    private readonly IList<string> _currentScrapingResults = new List<string>();
    
    protected WatchdogAlert() {}

    public WatchdogAlert(
        Watchdog watchdog,
        string? searchTerm
    )
    {
        Watchdog = watchdog;
        SearchTerm = searchTerm;

        var scrapingResults = watchdog.WebPages.SelectMany(x => x.ScrapingResults);
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            scrapingResults = scrapingResults.Where(x => x.ContainsIgnoringDiacritics(searchTerm));
        }
        _currentScrapingResults.AddRange(scrapingResults);
    }
    
    public virtual Watchdog Watchdog { get; } = null!;
    public virtual string? SearchTerm { get; protected set; }
    public virtual IEnumerable<string> PreviousScrapingResults => _previousScrapingResults;
    public virtual IEnumerable<string> CurrentScrapingResults => _currentScrapingResults;

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