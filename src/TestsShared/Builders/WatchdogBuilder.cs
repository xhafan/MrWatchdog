using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogBuilder(NhibernateUnitOfWork? unitOfWork)
{
    public const string Name = "watchdog name";

    private string _name = Name;

    public WatchdogBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public Watchdog Build()
    {
        var watchdog = new Watchdog(_name);

        unitOfWork?.Save(watchdog);
        
        return watchdog;
    }
}