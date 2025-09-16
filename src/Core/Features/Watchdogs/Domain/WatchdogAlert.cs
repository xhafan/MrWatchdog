using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogAlertScrapingResultsUpdated;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Resources;
using Serilog;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class WatchdogAlert : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _currentScrapingResults = new List<string>();
    private readonly IList<string> _scrapingResultsToAlertAbout = new List<string>();
    private readonly ISet<ScrapingResultAlertHistory> _scrapingResultsAlertHistory = new HashSet<ScrapingResultAlertHistory>();
    
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
    public virtual IEnumerable<ScrapingResultAlertHistory> ScrapingResultsAlertHistory => _scrapingResultsAlertHistory;

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
        Log.Information("Watchdog alert refresh, Id {Id}, SearchTerm {SearchTerm}, _currentScrapingResults: {_currentScrapingResults}", Id, SearchTerm, JsonHelper.Serialize(_currentScrapingResults));
        
        var newScrapingResultsSincePreviousScrapingResults = _currentScrapingResults.Except(previousScrapingResults).ToList();
        Log.Information("Watchdog alert refresh, Id {Id}, SearchTerm {SearchTerm}, newScrapingResultsSincePreviousScrapingResults: {newScrapingResultsSincePreviousScrapingResults}", Id, SearchTerm, JsonHelper.Serialize(newScrapingResultsSincePreviousScrapingResults));
        var newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout = newScrapingResultsSincePreviousScrapingResults.Except(_scrapingResultsToAlertAbout).ToList();
        Log.Information("Watchdog alert refresh, Id {Id}, SearchTerm {SearchTerm}, newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout: {newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout}", Id, SearchTerm, JsonHelper.Serialize(newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout));
       
        _addNewScrapingResultToAlertAboutOnlyIfTheResultHasNotBeenAlertedAboutRecently();
        
        if (_scrapingResultsToAlertAbout.Any())
        {
            DomainEvents.RaiseEvent(new WatchdogAlertScrapingResultsUpdatedDomainEvent(Id));
        } 

        Log.Information("Watchdog alert refresh, Id {Id}, SearchTerm {SearchTerm}, _scrapingResultsToAlertAbout: {_scrapingResultsToAlertAbout}", Id, SearchTerm, JsonHelper.Serialize(_scrapingResultsToAlertAbout));
        return;

        void _addNewScrapingResultToAlertAboutOnlyIfTheResultHasNotBeenAlertedAboutRecently()
        {
            _clearOldItemsInScrapingResultsAlertHistory();
            
            foreach (var newScrapingResultNotAlreadyInScrapingResultsToAlertAbout in newScrapingResultsNotAlreadyInScrapingResultsToAlertAbout)
            {
                if (_scrapingResultsAlertHistory.All(x => x.Result != newScrapingResultNotAlreadyInScrapingResultsToAlertAbout))
                {
                    _scrapingResultsToAlertAbout.Add(newScrapingResultNotAlreadyInScrapingResultsToAlertAbout);
                }
            }
        }
        
        void _clearOldItemsInScrapingResultsAlertHistory()
        {
            var historyExpiresOn = Clock.UtcNow.AddDays(-Watchdog.IntervalBetweenSameResultAlertsInDays);
            foreach (var scrapingResultAlertHistory in _scrapingResultsAlertHistory.ToList())
            {
                var isHistoryRecordExpired = scrapingResultAlertHistory.AlertedOn < historyExpiresOn;
                if (isHistoryRecordExpired)
                {
                    _scrapingResultsAlertHistory.Remove(scrapingResultAlertHistory);
                }
            }
        }  
    }

    public virtual async Task AlertUserAboutNewScrapingResults(
        IEmailSender emailSender,
        RuntimeOptions runtimeOptions
    )
    {
        var newScrapingResults = _scrapingResultsToAlertAbout.ToList();
        
        if (newScrapingResults.IsEmpty()) return;

        var mrWatchdogResource = Resource.MrWatchdog;
        var watchdogAlertName = $"{(!string.IsNullOrWhiteSpace(SearchTerm) ? $"{SearchTerm} - " : "")}{Watchdog.Name}";
        
        await emailSender.SendEmail(
            User.Email,
            $"{mrWatchdogResource}: new results for the watchdog alert {watchdogAlertName}",
            $"""
             <p>
                 Hello,
             </p>
             <p>
                 New results have been found for your alert <a href="{runtimeOptions.Url}{WatchdogUrlConstants.WatchdogAlertUrlTemplate.WithWatchdogAlertId(Id)}">{watchdogAlertName}</a>:
             </p>
             <ul>
                 {string.Join("\n    ", _scrapingResultsToAlertAbout.Select(scrapingResult => $"<li>{scrapingResult}</li>"))}
             </ul>
             <p>
                 Kind regards,<br>
                 {mrWatchdogResource}
             </p>
             """
        );

        foreach (var scrapingResult in _scrapingResultsToAlertAbout)
        {
            _scrapingResultsAlertHistory.Add(new ScrapingResultAlertHistory(scrapingResult, Clock.UtcNow));
        }
        
        _scrapingResultsToAlertAbout.Clear();
    }
}