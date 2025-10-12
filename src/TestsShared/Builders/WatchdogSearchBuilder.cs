using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogSearchBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string SearchTerm = "search term";

    private Watchdog? _watchdog;
    private User? _user;
    private string? _searchTerm = SearchTerm;

    public WatchdogSearchBuilder WithWatchdog(Watchdog watchdog)
    {
        _watchdog = watchdog;
        return this;
    } 
    
    public WatchdogSearchBuilder WithUser(User user)
    {
        _user = user;
        return this;
    } 

    public WatchdogSearchBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    } 

    public  WatchdogSearch Build()
    {
        _watchdog ??= new WatchdogBuilder(unitOfWork).Build();
        _user ??= new UserBuilder(unitOfWork).Build();

        var watchdogSearch = new WatchdogSearch(_watchdog, _user, _searchTerm);

        if (unitOfWork == null)
        {
            watchdogSearch.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
        }    
        
        unitOfWork?.Save(watchdogSearch);
        
        return watchdogSearch;
    }
}