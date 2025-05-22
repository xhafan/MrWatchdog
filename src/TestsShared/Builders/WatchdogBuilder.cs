using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class WatchdogBuilder(NhibernateUnitOfWork? unitOfWork)
{
    public Watchdog Build()
    {
        var watchdog = new Watchdog("watchdog name");

        unitOfWork?.Save(watchdog);
        
        return watchdog;
    }
}