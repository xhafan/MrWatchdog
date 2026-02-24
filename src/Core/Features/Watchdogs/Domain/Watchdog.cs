using CoreDdd.Domain;
using CoreDdd.Domain.Events;
using CoreUtils.Extensions;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Shared.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogArchived;
using MrWatchdog.Core.Features.Watchdogs.Domain.Events.WatchdogScrapedResultsUpdated;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.Core.Infrastructure.Configurations;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.Core.Infrastructure.Extensions;
using MrWatchdog.Core.Infrastructure.Jsons;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Resources;
using Serilog;

namespace MrWatchdog.Core.Features.Watchdogs.Domain;

public class Watchdog : VersionedEntity, IAggregateRoot
{
    private readonly IList<ScrapedResult> _currentScrapedResults = new List<ScrapedResult>();
    private readonly IList<ScrapedResult> _scrapedResultsToNotifyAbout = new List<ScrapedResult>();
    private readonly ISet<WatchdogScrapedResultHistory> _scrapedResultsHistory = new HashSet<WatchdogScrapedResultHistory>();
    
    protected Watchdog() {}

    public Watchdog(
        Scraper scraper,
        User user,
        string? searchTerm
    )
    {
        Scraper = scraper;
        User = user;
        ReceiveNotification = true;
        SearchTerm = searchTerm;

        var scrapedResults = _GetScraperScrapedResults();
        
        _currentScrapedResults.AddRange(scrapedResults.Select(x => new ScrapedResult(x)));
    }

    public virtual Scraper Scraper { get; } = null!;
    public virtual User User { get; } = null!;
    public virtual bool ReceiveNotification { get; protected set; }
    public virtual string? SearchTerm { get; protected set; }
    public virtual IEnumerable<ScrapedResult> CurrentScrapedResults => _currentScrapedResults;
    public virtual IEnumerable<ScrapedResult> ScrapedResultsToNotifyAbout => _scrapedResultsToNotifyAbout;
    public virtual IEnumerable<WatchdogScrapedResultHistory> ScrapedResultsHistory => _scrapedResultsHistory;
    public virtual bool IsArchived { get; protected set; }


    private IEnumerable<string> _GetScraperScrapedResults()
    {
        return Scraper.WebPages
            .Where(x => x.IsEnabled)
            .SelectMany(x => x.ScrapedResults)
            .Where(x => string.IsNullOrWhiteSpace(SearchTerm) || x.ContainsIgnoringDiacritics(SearchTerm));
    }
    
    public virtual WatchdogArgs GetWatchdogArgs()
    {
        return new WatchdogArgs
        {
            WatchdogId = Id,
            ScraperId = Scraper.Id,
            SearchTerm = SearchTerm,
            ScraperPublicStatus = Scraper.PublicStatus,
            IsArchived = IsArchived
        };
    }

    public virtual WatchdogOverviewArgs GetWatchdogOverviewArgs()
    {
        return new WatchdogOverviewArgs
        {
            WatchdogId = Id,
            ReceiveNotification = ReceiveNotification,
            SearchTerm = SearchTerm
        };
    }
    
    public virtual WatchdogScraperDto GetWatchdogScraperDto()
    {
        return new WatchdogScraperDto
        {
            WatchdogId = Id,
            ScrapedResultsFilteringNotSupported = Scraper.ScrapedResultsFilteringNotSupported
        };
    }    

    public virtual void UpdateOverview(WatchdogOverviewArgs args)
    {
        ReceiveNotification = args.ReceiveNotification;
        SearchTerm = args.SearchTerm;
    }

    public virtual void Refresh()
    {
        var previousScrapedResults = _currentScrapedResults.ToList();
        
        _currentScrapedResults.Clear();
        _currentScrapedResults.AddRange(_GetScraperScrapedResults().Select(x => new ScrapedResult(x)));
        Log.Information("Watchdog refresh, Id {Id}, SearchTerm {SearchTerm}, _currentScrapedResults: {_currentScrapedResults}", Id, SearchTerm, JsonHelper.Serialize(_currentScrapedResults));

        var newScrapedResultsSincePreviousScrapedResults = _currentScrapedResults.Except(previousScrapedResults).ToList();
        Log.Information("Watchdog refresh, Id {Id}, SearchTerm {SearchTerm}, newScrapedResultsSincePreviousScrapedResults: {newScrapedResultsSincePreviousScrapedResults}", Id, SearchTerm, JsonHelper.Serialize(newScrapedResultsSincePreviousScrapedResults));
        
        var newScrapedResultsNotAlreadyInScrapedResultsToNotifyAbout = newScrapedResultsSincePreviousScrapedResults.Except(_scrapedResultsToNotifyAbout).ToList();
        Log.Information("Watchdog refresh, Id {Id}, SearchTerm {SearchTerm}, newScrapedResultsNotAlreadyInScrapedResultsToNotifyAbout: {newScrapedResultsNotAlreadyInScrapedResultsToNotifyAbout}", Id, SearchTerm, JsonHelper.Serialize(newScrapedResultsNotAlreadyInScrapedResultsToNotifyAbout));
        
        _addNewScrapedResultToNotifyAboutOnlyIfTheResultHasNotBeenNotifiedAboutRecently();
        
        if (_scrapedResultsToNotifyAbout.Any())
        {
            DomainEvents.RaiseEvent(new WatchdogScrapedResultsUpdatedDomainEvent(Id));
        } 

        Log.Information("Watchdog refresh, Id {Id}, SearchTerm {SearchTerm}, _scrapedResultsToNotifyAbout: {_scrapedResultsToNotifyAbout}", Id, SearchTerm, JsonHelper.Serialize(_scrapedResultsToNotifyAbout));
        return;

        void _addNewScrapedResultToNotifyAboutOnlyIfTheResultHasNotBeenNotifiedAboutRecently()
        {
            _clearOldItemsInScrapedResultsHistory();

            var scrapedResultsHistory = _scrapedResultsHistory.Select(x => x.ScrapedResult).ToHashSet();

            foreach (var newScrapedResultNotAlreadyInScrapedResultsToNotifyAbout in newScrapedResultsNotAlreadyInScrapedResultsToNotifyAbout)
            {
                if (!scrapedResultsHistory.Contains(newScrapedResultNotAlreadyInScrapedResultsToNotifyAbout))
                {
                    _scrapedResultsToNotifyAbout.Add(newScrapedResultNotAlreadyInScrapedResultsToNotifyAbout);
                }
            }
        }
        
        void _clearOldItemsInScrapedResultsHistory()
        {
            var historyExpiresOn = Clock.UtcNow.AddDays(-Scraper.IntervalBetweenSameResultNotificationsInDays);
            foreach (var scrapedResultHistory in _scrapedResultsHistory.ToList())
            {
                var isHistoryRecordExpired = scrapedResultHistory.NotifiedOn < historyExpiresOn;
                if (isHistoryRecordExpired)
                {
                    _scrapedResultsHistory.Remove(scrapedResultHistory);
                }
            }
        }  
    }

    public virtual async Task NotifyUserAboutNewScrapedResults(
        ICoreBus bus,
        RuntimeOptions runtimeOptions,
        JwtOptions jwtOptions
    )
    {
        var newScrapedResults = _scrapedResultsToNotifyAbout.ToList();
        
        if (newScrapedResults.IsEmpty()) return;

        if (ReceiveNotification)
        {
            var userCulture = User.Culture;

            var mrWatchdogResource = ResourceHelper.GetString(nameof(Resource.MrWatchdog), userCulture);
            var searchTermSuffix = !string.IsNullOrWhiteSpace(SearchTerm) ? $" - {SearchTerm}" : "";
            
            var localizedScraperName = Scraper.GetLocalizedName(userCulture);

            var unsubscribeToken = TokenGenerator.GenerateUnsubscribeToken(Id, jwtOptions);

            await bus.Send(new SendEmailCommand(
                User.Email,
                string.Format(
                    ResourceHelper.GetString(nameof(Resource.NewScrapedResultsEmailSubject), userCulture), 
                    mrWatchdogResource, 
                    localizedScraperName, 
                    searchTermSuffix
                ),
                string.Format(
                    ResourceHelper.GetString(nameof(Resource.NewScrapedResultsEmailBody), userCulture), 
                    runtimeOptions.Url, 
                    WatchdogUrlConstants.WatchdogDetailUrlTemplate.WithWatchdogId(Id),
                    localizedScraperName,
                    searchTermSuffix,
                    string.Join("\n    ", _scrapedResultsToNotifyAbout.Select(scrapingResult => $"<li>{scrapingResult.Value}</li>")),
                    mrWatchdogResource,
                    $"{runtimeOptions.Url}{WatchdogUrlConstants.DisableWatchdogNotificationsGetUrlTemplate.WithUnsubscribeToken(unsubscribeToken)}",
                    ResourceHelper.GetString(nameof(Resource.Unsubscribe), userCulture)
                ),
                UnsubscribeUrl: $"{runtimeOptions.Url}{WatchdogUrlConstants.DisableWatchdogNotificationsPostUrlTemplate.WithUnsubscribeToken(unsubscribeToken)}"
            ));

            foreach (var scrapedResult in _scrapedResultsToNotifyAbout)
            {
                _scrapedResultsHistory.Add(new WatchdogScrapedResultHistory(scrapedResult, Clock.UtcNow));
            }
        }

        _scrapedResultsToNotifyAbout.Clear();
    }

    public virtual void Archive(User? actingUser = null)
    {
        IsArchived = true;

        if (actingUser != User)
        {
            DomainEvents.RaiseEvent(new WatchdogArchivedDomainEvent(Id));
        }
    }

    public virtual async Task NotifyUserAboutWatchdogArchived(ICoreBus bus)
    {
        var userCulture = User.Culture;

        var mrWatchdogResource = ResourceHelper.GetString(nameof(Resource.MrWatchdog), userCulture);
        var scraperLocalizedName = Scraper.GetLocalizedName(userCulture);
        var watchdogName = $"{scraperLocalizedName}{(!string.IsNullOrWhiteSpace(SearchTerm) ? $" - {SearchTerm}" : "")}";

        await bus.Send(new SendEmailCommand(
            User.Email,
            string.Format(ResourceHelper.GetString(nameof(Resource.WatchdogArchivedEmailSubject), userCulture), mrWatchdogResource, watchdogName),
            string.Format(
                ResourceHelper.GetString(nameof(Resource.WatchdogArchivedEmailBody), userCulture), 
                watchdogName,
                scraperLocalizedName,
                mrWatchdogResource
            )
        ));
    }

    public virtual void DisableNotification()
    {
        ReceiveNotification = false;
    }

    public virtual void RefreshScrapedResultsHashes()
    {
        foreach (var scrapedResult in _currentScrapedResults)
        {
            scrapedResult.RecalculateHash();
        }

        foreach (var scrapedResult in _scrapedResultsToNotifyAbout)
        {
            scrapedResult.RecalculateHash();
        }

        foreach (var scrapedResult in _scrapedResultsHistory)
        {
            scrapedResult.ScrapedResult.RecalculateHash();
        }
    }
}