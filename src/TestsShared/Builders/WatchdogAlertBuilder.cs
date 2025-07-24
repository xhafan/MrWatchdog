using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogAlertBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string SearchTerm = "search term";

    private Watchdog? _watchdog;
    private string? _searchTerm = SearchTerm;

    public WatchdogAlertBuilder WithWatchdog(Watchdog watchdog)
    {
        _watchdog = watchdog;
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

        var watchdogAlert = new WatchdogAlert(_watchdog, _searchTerm);

        if (unitOfWork == null)
        {
            watchdogAlert.SetPrivateProperty(x => x.Id, Incrementer.GetNextIncrement());
        }    
        
        unitOfWork?.Save(watchdogAlert);
        
        return watchdogAlert;
    }
}