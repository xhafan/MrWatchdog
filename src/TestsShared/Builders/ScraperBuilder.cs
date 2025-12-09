using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class ScraperBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string Name = "scraper name";
    public const string Description = "scraper description";
    public const int ScrapingIntervalInSeconds = 60;
    public const double IntervalBetweenSameResultNotificationsInDays = 20;
    public const int NumberOfFailedScrapingAttemptsBeforeAlerting = 5;

    private User? _user;
    private string _name = Name;
    private string _description = Description;
    private int _scrapingIntervalInSeconds = ScrapingIntervalInSeconds;
    private double _intervalBetweenSameResultNotificationsInDays = IntervalBetweenSameResultNotificationsInDays;

    private ScraperWebPageArgs[]? _scraperWebPageArgses;
    private DateTime? _nextScrapingOn;
    private int _numberOfFailedScrapingAttemptsBeforeAlerting = NumberOfFailedScrapingAttemptsBeforeAlerting;

    public ScraperBuilder WithUser(User user)
    {
        _user = user;
        return this;
    }

    public ScraperBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ScraperBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public ScraperBuilder WithScrapingIntervalInSeconds(int scrapingIntervalInSeconds)
    {
        _scrapingIntervalInSeconds = scrapingIntervalInSeconds;
        return this;
    }

    public ScraperBuilder WithIntervalBetweenSameResultNotificationsInDays(double intervalBetweenSameResultNotificationsInDays)
    {
        _intervalBetweenSameResultNotificationsInDays = intervalBetweenSameResultNotificationsInDays;
        return this;
    }

    public ScraperBuilder WithWebPage(params ScraperWebPageArgs[] scraperWebPageArgses)
    {
        _scraperWebPageArgses = scraperWebPageArgses;
        return this;
    } 
    
    public ScraperBuilder WithNextScrapingOn(DateTime? nextScrapingOn)
    {
        _nextScrapingOn = nextScrapingOn;
        return this;
    }

    public ScraperBuilder WithNumberOfFailedScrapingAttemptsBeforeAlerting(int numberOfFailedScrapingAttemptsBeforeAlerting)
    {
        _numberOfFailedScrapingAttemptsBeforeAlerting = numberOfFailedScrapingAttemptsBeforeAlerting;
        return this;
    }
    
    public Scraper Build()
    {
        _user ??= new UserBuilder(unitOfWork).Build();
        
        var scraper = new Scraper(_user, _name);

        if (_scraperWebPageArgses != null)
        {
            foreach (var scraperWebPageArgs in _scraperWebPageArgses)
            {
                scraper.AddWebPage(scraperWebPageArgs);
            }
        }

        if (unitOfWork == null)
        {
            scraper.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
            foreach (var scraperWebPage in scraper.WebPages)
            {
                scraperWebPage.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
            }
        }        
        
        unitOfWork?.Save(scraper);
        
        scraper.UpdateOverview(new ScraperOverviewArgs
        {
            ScraperId = scraper.Id,
            Name = _name, 
            Description = _description,
            ScrapingIntervalInSeconds = _scrapingIntervalInSeconds,
            IntervalBetweenSameResultNotificationsInDays = _intervalBetweenSameResultNotificationsInDays,
            NumberOfFailedScrapingAttemptsBeforeAlerting = _numberOfFailedScrapingAttemptsBeforeAlerting
        });
        
        scraper.SetNextScrapingOn(_nextScrapingOn);
        
        return scraper;
    }
}