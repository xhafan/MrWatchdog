using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string Name = "watchdog name";

    private string _name = Name;
    private WatchdogWebPageArgs[]? _watchdogWebPageArgses;

    public WatchdogBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public WatchdogBuilder WithWebPage(params WatchdogWebPageArgs[] watchdogWebPageArgses)
    {
        _watchdogWebPageArgses = watchdogWebPageArgses;
        return this;
    } 
    

    public Watchdog Build()
    {
        var watchdog = new Watchdog(_name);

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
        
        return watchdog;
    }
}