using CoreDdd.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure.Extensions;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _previousScrapingResults = new List<string>();
    private readonly IList<string> _currentScrapingResults = new List<string>();
    private readonly IList<string> _scrapingResultsToAlertAbout = new List<string>();
    
    protected WatchdogAlert() {}

    public WatchdogAlert(
        Watchdog watchdog,
        string? searchTerm
    )
    {
        Watchdog = watchdog;
        SearchTerm = searchTerm;

        var scrapingResults = _GetWatchdogScrapingResults();
        
        _currentScrapingResults.AddRange(scrapingResults);
    }

    public virtual Watchdog Watchdog { get; } = null!;
    public virtual string? SearchTerm { get; protected set; }
    public virtual IEnumerable<string> PreviousScrapingResults => _previousScrapingResults;
    public virtual IEnumerable<string> CurrentScrapingResults => _currentScrapingResults;
    public virtual IEnumerable<string> ScrapingResultsToAlertAbout => _scrapingResultsToAlertAbout;

    private IEnumerable<string> _GetWatchdogScrapingResults()
    {
        return Watchdog.WebPages.SelectMany(x => x.ScrapingResults)
            .Where(x => string.IsNullOrWhiteSpace(SearchTerm) || x.ContainsIgnoringDiacritics(SearchTerm));
    }
    
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

    public virtual void Refresh()
    {
        _previousScrapingResults.Clear();
        _previousScrapingResults.AddRange(_currentScrapingResults);
        
        _currentScrapingResults.Clear();
        _currentScrapingResults.AddRange(_GetWatchdogScrapingResults());
        
        _scrapingResultsToAlertAbout.AddRange(_currentScrapingResults.Except(_previousScrapingResults));
    }
}