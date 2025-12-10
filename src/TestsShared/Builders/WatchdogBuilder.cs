using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogSearchBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const bool ReceiveNotification = true;
    public const string SearchTerm = "search term";

    private Scraper? _scraper;
    private User? _user;
    private bool _receiveNotification = ReceiveNotification;
    private string? _searchTerm = SearchTerm;

    public WatchdogSearchBuilder WithScraper(Scraper scraper)
    {
        _scraper = scraper;
        return this;
    } 
    
    public WatchdogSearchBuilder WithUser(User user)
    {
        _user = user;
        return this;
    } 

    public WatchdogSearchBuilder WithReceiveNotification(bool receiveNotification)
    {
        _receiveNotification = receiveNotification;
        return this;
    } 

    public WatchdogSearchBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    } 

    public  WatchdogSearch Build()
    {
        _scraper ??= new ScraperBuilder(unitOfWork).Build();
        _user ??= new UserBuilder(unitOfWork).Build();

        var watchdogSearch = new WatchdogSearch(_scraper, _user, _searchTerm);
        watchdogSearch.UpdateOverview(new WatchdogSearchOverviewArgs
        {
            WatchdogSearchId = 0,
            ReceiveNotification = _receiveNotification,
            SearchTerm = _searchTerm
        });

        if (unitOfWork == null)
        {
            watchdogSearch.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
        }    
        
        unitOfWork?.Save(watchdogSearch);
        
        return watchdogSearch;
    }
}