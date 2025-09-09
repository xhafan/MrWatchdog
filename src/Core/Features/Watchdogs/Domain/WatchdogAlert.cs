using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Extensions;
using Serilog;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _currentScrapingResults = new List<string>();
    private readonly IList<string> _scrapingResultsToAlertAbout = new List<string>();
    
    protected WatchdogAlert() {}

    public WatchdogAlert(
        Watchdog watchdog,
        User user,
        string? searchTerm
    )
    {
        Watchdog = watchdog;
        User = user;
        SearchTerm = searchTerm;

        var scrapingResults = _GetWatchdogScrapingResults();
        
        _currentScrapingResults.AddRange(scrapingResults);
    }

    public virtual Watchdog Watchdog { get; } = null!;
    public virtual User User { get; } = null!;
    public virtual string? SearchTerm { get; protected set; }
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
        var previousScrapingResults = _currentScrapingResults.ToList();
        
        _currentScrapingResults.Clear();
        _currentScrapingResults.AddRange(_GetWatchdogScrapingResults());
        Log.Information("Watchdog alert {Id} _currentScrapingResults: {currentScrapingResults}", Id, JsonHelper.Serialize(_currentScrapingResults)); // todo: remove once not needed
        
        var newScrapingResultsSincePreviousScrapingResults = _currentScrapingResults.Except(previousScrapingResults).ToList();
        Log.Information("Watchdog alert {Id} newScrapingResultsSincePreviousScrapingResults: {currentScrapingResults}", Id, JsonHelper.Serialize(newScrapingResultsSincePreviousScrapingResults)); // todo: remove once not needed
        var newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout = newScrapingResultsSincePreviousScrapingResults.Except(_scrapingResultsToAlertAbout).ToList();
        Log.Information("Watchdog alert {Id} newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout: {currentScrapingResults}", Id, JsonHelper.Serialize(newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout)); // todo: remove once not needed
            
        if (newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout.Any())
        {
            DomainEvents.RaiseEvent(new WatchdogAlertScrapingResultsUpdatedDomainEvent(Id));
        }
        
        _scrapingResultsToAlertAbout.AddRange(newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout);
    }

    public virtual void ClearScrapingResultsToAlertAbout()
    {
        _scrapingResultsToAlertAbout.Clear();
    }
}