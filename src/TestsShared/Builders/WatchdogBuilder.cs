using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const bool ReceiveNotification = true;
    public const string SearchTerm = "search term";

    private Scraper? _scraper;
    private User? _user;
    private bool _receiveNotification = ReceiveNotification;
    private string? _searchTerm = SearchTerm;

    public WatchdogBuilder WithScraper(Scraper scraper)
    {
        _scraper = scraper;
        return this;
    } 
    
    public WatchdogBuilder WithUser(User user)
    {
        _user = user;
        return this;
    } 

    public WatchdogBuilder WithReceiveNotification(bool receiveNotification)
    {
        _receiveNotification = receiveNotification;
        return this;
    } 

    public WatchdogBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    } 

    public Watchdog Build()
    {
        _scraper ??= new ScraperBuilder(unitOfWork).Build();
        _user ??= new UserBuilder(unitOfWork).Build();

        var watchdog = new Watchdog(_scraper, _user, _searchTerm);
        watchdog.UpdateOverview(new WatchdogOverviewArgs
        {
            WatchdogId = 0,
            ReceiveNotification = _receiveNotification,
            SearchTerm = _searchTerm
        });

        if (unitOfWork == null)
        {
            watchdog.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
        }    
        
        unitOfWork?.Save(watchdog);
        
        return watchdog;
    }
}