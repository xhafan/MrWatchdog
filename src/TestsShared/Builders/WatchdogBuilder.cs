using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string Name = "watchdog name";
    public const string Description = "watchdog description";
    public const int ScrapingIntervalInSeconds = 60;
    public const double IntervalBetweenSameResultNotificationsInDays = 20;
    public const int NumberOfFailedScrapingAttemptsBeforeAlerting = 5;

    private User? _user;
    private string _name = Name;
    private string _description = Description;
    private int _scrapingIntervalInSeconds = ScrapingIntervalInSeconds;
    private double _intervalBetweenSameResultNotificationsInDays = IntervalBetweenSameResultNotificationsInDays;

    private WatchdogWebPageArgs[]? _watchdogWebPageArgses;
    private DateTime? _nextScrapingOn;
    private int _numberOfFailedScrapingAttemptsBeforeAlerting = NumberOfFailedScrapingAttemptsBeforeAlerting;

    public WatchdogBuilder WithUser(User user)
    {
        _user = user;
        return this;
    }

    public WatchdogBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public WatchdogBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public WatchdogBuilder WithScrapingIntervalInSeconds(int scrapingIntervalInSeconds)
    {
        _scrapingIntervalInSeconds = scrapingIntervalInSeconds;
        return this;
    }

    public WatchdogBuilder WithIntervalBetweenSameResultNotificationsInDays(double intervalBetweenSameResultNotificationsInDays)
    {
        _intervalBetweenSameResultNotificationsInDays = intervalBetweenSameResultNotificationsInDays;
        return this;
    }

    public WatchdogBuilder WithWebPage(params WatchdogWebPageArgs[] watchdogWebPageArgses)
    {
        _watchdogWebPageArgses = watchdogWebPageArgses;
        return this;
    } 
    
    public WatchdogBuilder WithNextScrapingOn(DateTime? nextScrapingOn)
    {
        _nextScrapingOn = nextScrapingOn;
        return this;
    }

    public WatchdogBuilder WithNumberOfFailedScrapingAttemptsBeforeAlerting(int numberOfFailedScrapingAttemptsBeforeAlerting)
    {
        _numberOfFailedScrapingAttemptsBeforeAlerting = numberOfFailedScrapingAttemptsBeforeAlerting;
        return this;
    }
    
    public Watchdog Build()
    {
        _user ??= new UserBuilder(unitOfWork).Build();
        
        var watchdog = new Watchdog(_user, _name);

        if (_watchdogWebPageArgses != null)
        {
            foreach (var watchdogWebPageArgs in _watchdogWebPageArgses)
            {
                watchdog.AddWebPage(watchdogWebPageArgs);
            }
        }

        if (unitOfWork == null)
        {
            watchdog.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
            foreach (var watchdogWebPage in watchdog.WebPages)
            {
                watchdogWebPage.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
            }
        }        
        
        unitOfWork?.Save(watchdog);
        
        watchdog.UpdateOverview(new WatchdogOverviewArgs
        {
            WatchdogId = watchdog.Id,
            Name = _name, 
            Description = _description,
            ScrapingIntervalInSeconds = _scrapingIntervalInSeconds,
            IntervalBetweenSameResultNotificationsInDays = _intervalBetweenSameResultNotificationsInDays,
            NumberOfFailedScrapingAttemptsBeforeAlerting = _numberOfFailedScrapingAttemptsBeforeAlerting
        });
        
        watchdog.SetNextScrapingOn(_nextScrapingOn);
        
        return watchdog;
    }
}