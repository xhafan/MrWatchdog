using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogAlertBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string SearchTerm = "search term";

    private Watchdog? _watchdog;
    private User? _user;
    private string? _searchTerm = SearchTerm;

    public WatchdogAlertBuilder WithWatchdog(Watchdog watchdog)
    {
        _watchdog = watchdog;
        return this;
    } 
    
    public WatchdogAlertBuilder WithUser(User user)
    {
        _user = user;
        return this;
    } 

    public WatchdogAlertBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    } 

    public  WatchdogAlert Build()
    {
        _watchdog ??= new WatchdogBuilder(unitOfWork).Build();
        _user ??= new UserBuilder(unitOfWork).Build();

        var watchdogAlert = new WatchdogAlert(_watchdog, _user, _searchTerm);

        if (unitOfWork == null)
        {
            watchdogAlert.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
        }    
        
        unitOfWork?.Save(watchdogAlert);
        
        return watchdogAlert;
    }
}