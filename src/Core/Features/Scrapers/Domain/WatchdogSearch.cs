using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchArchived;
using MrWatchdog.Core.Features.Scrapers.Domain.Events.WatchdogSearchScrapingResultsUpdated;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Resources;
using Serilog;

namespace MrWatchdog.Core.Features.Scrapers.Domain;

public class WatchdogSearch : VersionedEntity, IAggregateRoot
{
    private readonly IList<string> _currentScrapingResults = new List<string>();
    private readonly IList<string> _scrapingResultsToNotifyAbout = new List<string>();
    private readonly ISet<ScrapingResultHistory> _scrapingResultsHistory = new HashSet<ScrapingResultHistory>();
    
    protected WatchdogSearch() {}

    public WatchdogSearch(
        Scraper scraper,
        User user,
        string? searchTerm
    )
    {
        Scraper = scraper;
        User = user;
        ReceiveNotification = true;
        SearchTerm = searchTerm;

        var scrapingResults = _GetScraperScrapingResults();
        
        _currentScrapingResults.AddRange(scrapingResults);
    }

    public virtual Scraper Scraper { get; } = null!;
    public virtual User User { get; } = null!;
    public virtual bool ReceiveNotification { get; protected set; }
    public virtual string? SearchTerm { get; protected set; }
    public virtual IEnumerable<string> CurrentScrapingResults => _currentScrapingResults;
    public virtual IEnumerable<string> ScrapingResultsToNotifyAbout => _scrapingResultsToNotifyAbout;
    public virtual IEnumerable<ScrapingResultHistory> ScrapingResultsHistory => _scrapingResultsHistory;
    public virtual bool IsArchived { get; protected set; }


    private IEnumerable<string> _GetScraperScrapingResults()
    {
        return Scraper.WebPages
            .Where(x => x.IsEnabled)
            .SelectMany(x => x.ScrapingResults)
            .Where(x => string.IsNullOrWhiteSpace(SearchTerm) || x.ContainsIgnoringDiacritics(SearchTerm));
    }
    
    public virtual WatchdogSearchArgs GetWatchdogSearchArgs()
    {
        return new WatchdogSearchArgs
        {
            WatchdogSearchId = Id,
            ScraperId = Scraper.Id,
            SearchTerm = SearchTerm,
            ScraperPublicStatus = Scraper.PublicStatus,
            IsArchived = IsArchived
        };
    }

    public virtual WatchdogSearchOverviewArgs GetWatchdogSearchOverviewArgs()
    {
        return new WatchdogSearchOverviewArgs
        {
            WatchdogSearchId = Id,
            ReceiveNotification = ReceiveNotification,
            SearchTerm = SearchTerm
        };
    }

    public virtual void UpdateOverview(WatchdogSearchOverviewArgs args)
    {
        ReceiveNotification = args.ReceiveNotification;
        SearchTerm = args.SearchTerm;
    }

    public virtual void Refresh()
    {
        var previousScrapingResults = _currentScrapingResults.ToList();
        
        _currentScrapingResults.Clear();
        _currentScrapingResults.AddRange(_GetScraperScrapingResults());
        Log.Information("Watchdog search refresh, Id {Id}, SearchTerm {SearchTerm}, _currentScrapingResults: {_currentScrapingResults}", Id, SearchTerm, JsonHelper.Serialize(_currentScrapingResults));
        
        var newScrapingResultsSincePreviousScrapingResults = _currentScrapingResults.Except(previousScrapingResults).ToList();
        Log.Information("Watchdog search refresh, Id {Id}, SearchTerm {SearchTerm}, newScrapingResultsSincePreviousScrapingResults: {newScrapingResultsSincePreviousScrapingResults}", Id, SearchTerm, JsonHelper.Serialize(newScrapingResultsSincePreviousScrapingResults));
        var newScrapingResultsNotAlreadyInScrapingResultsToNotifyAbout = newScrapingResultsSincePreviousScrapingResults.Except(_scrapingResultsToNotifyAbout).ToList();
        Log.Information("Watchdog search refresh, Id {Id}, SearchTerm {SearchTerm}, newScrapingResultsNotAlreadyInScrapingResultsToNotifyAbout: {newScrapingResultsNotAlreadyInScrapingResultsToNotifyAbout}", Id, SearchTerm, JsonHelper.Serialize(newScrapingResultsNotAlreadyInScrapingResultsToNotifyAbout));
       
        _addNewScrapingResultToNotifyAboutOnlyIfTheResultHasNotBeenNotifiedAboutRecently();
        
        if (_scrapingResultsToNotifyAbout.Any())
        {
            DomainEvents.RaiseEvent(new WatchdogSearchScrapingResultsUpdatedDomainEvent(Id));
        } 

        Log.Information("Watchdog search refresh, Id {Id}, SearchTerm {SearchTerm}, _scrapingResultsToNotifyAbout: {_scrapingResultsToNotifyAbout}", Id, SearchTerm, JsonHelper.Serialize(_scrapingResultsToNotifyAbout));
        return;

        void _addNewScrapingResultToNotifyAboutOnlyIfTheResultHasNotBeenNotifiedAboutRecently()
        {
            _clearOldItemsInScrapingResultsHistory();
            
            foreach (var newScrapingResultNotAlreadyInScrapingResultsToNotifyAbout in newScrapingResultsNotAlreadyInScrapingResultsToNotifyAbout)
            {
                if (_scrapingResultsHistory.All(x => x.Result != newScrapingResultNotAlreadyInScrapingResultsToNotifyAbout))
                {
                    _scrapingResultsToNotifyAbout.Add(newScrapingResultNotAlreadyInScrapingResultsToNotifyAbout);
                }
            }
        }
        
        void _clearOldItemsInScrapingResultsHistory()
        {
            var historyExpiresOn = Clock.UtcNow.AddDays(-Scraper.IntervalBetweenSameResultNotificationsInDays);
            foreach (var scrapingResultHistory in _scrapingResultsHistory.ToList())
            {
                var isHistoryRecordExpired = scrapingResultHistory.NotifiedOn < historyExpiresOn;
                if (isHistoryRecordExpired)
                {
                    _scrapingResultsHistory.Remove(scrapingResultHistory);
                }
            }
        }  
    }

    public virtual async Task NotifyUserAboutNewScrapingResults(
        IEmailSender emailSender,
        RuntimeOptions runtimeOptions
    )
    {
        var newScrapingResults = _scrapingResultsToNotifyAbout.ToList();
        
        if (newScrapingResults.IsEmpty()) return;

        var mrWatchdogResource = Resource.MrWatchdog;
        var searchTermSuffix = (!string.IsNullOrWhiteSpace(SearchTerm) ? $" - {SearchTerm}" : "");

        if (ReceiveNotification)
        {
            await emailSender.SendEmail(
                User.Email,
                $"{mrWatchdogResource}: new results for {Scraper.Name}{searchTermSuffix}",
                $"""
                 <html>
                 <body>
                 <p>
                     Hello,
                 </p>
                 <p>
                     New results have been found for 
                     <a href="{runtimeOptions.Url}{ScraperUrlConstants.WatchdogSearchUrlTemplate.WithWatchdogSearchId(Id)}">
                        <span>{Scraper.Name}</span><span translate="no">{searchTermSuffix}</span>
                     </a>:
                 </p>
                 <ul>
                     {string.Join("\n    ", _scrapingResultsToNotifyAbout.Select(scrapingResult => $"<li>{scrapingResult}</li>"))}
                 </ul>
                 <p>
                     Kind regards,<br>
                     {mrWatchdogResource}
                 </p>
                 </body>
                 </html>
                 """
            );

            foreach (var scrapingResult in _scrapingResultsToNotifyAbout)
            {
                _scrapingResultsHistory.Add(new ScrapingResultHistory(scrapingResult, Clock.UtcNow));
            }
        }

        _scrapingResultsToNotifyAbout.Clear();
    }

    public virtual void Archive(User? actingUser = null)
    {
        IsArchived = true;

        if (actingUser != User)
        {
            DomainEvents.RaiseEvent(new WatchdogSearchArchivedDomainEvent(Id));
        }
    }

    public virtual async Task NotifyUserAboutWatchdogSearchArchived(
        IEmailSender emailSender
    )
    {
        var mrWatchdogResource = Resource.MrWatchdog;
        var watchdogSearchName = $"{Scraper.Name}{(!string.IsNullOrWhiteSpace(SearchTerm) ? $" - {SearchTerm}" : "")}";

        await emailSender.SendEmail(
            User.Email,
            $"{mrWatchdogResource}: your watchdog {watchdogSearchName} has been deleted",
            $"""
             <html>
             <body>
             <p>
                 Hello,
             </p>
             <p>
                 Your watchdog <b>{watchdogSearchName}</b> has been deleted due to scraper <b>{Scraper.Name}</b> deletion.
             </p>
             <p>
                 Kind regards,<br>
                 {mrWatchdogResource}
             </p>
             </body>
             </html>
             """
        );
    }

}