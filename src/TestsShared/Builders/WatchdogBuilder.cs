using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string Name = "watchdog name";
    public const int ScrapingIntervalInSeconds = 60;
    public const double IntervalBetweenSameResultAlertsInDays = 20;

    private User? _user;
    private string _name = Name;
    private int _scrapingIntervalInSeconds = ScrapingIntervalInSeconds;
    private double _intervalBetweenSameResultAlertsInDays = IntervalBetweenSameResultAlertsInDays;

    private WatchdogWebPageArgs[]? _watchdogWebPageArgses;
    private DateTime? _nextScrapingOn;

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
    
    public WatchdogBuilder WithScrapingIntervalInSeconds(int scrapingIntervalInSeconds)
    {
        _scrapingIntervalInSeconds = scrapingIntervalInSeconds;
        return this;
    }

    public WatchdogBuilder WithIntervalBetweenSameResultAlertsInDays(double intervalBetweenSameResultAlertsInDays)
    {
        _intervalBetweenSameResultAlertsInDays = intervalBetweenSameResultAlertsInDays;
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
            ScrapingIntervalInSeconds = _scrapingIntervalInSeconds,
            IntervalBetweenSameResultAlertsInDays = _intervalBetweenSameResultAlertsInDays
        });
        
        watchdog.SetNextScrapingOn(_nextScrapingOn);
        
        return watchdog;
    }
}