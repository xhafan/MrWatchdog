using System.Reflection;
using CoreDdd.Nhibernate.Configurations;
#if DEBUG
using HibernatingRhinos.Profiler.Appender.NHibernate;
#endif
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.Core.Infrastructure;

public class NhibernateConfigurator : BaseNhibernateConfigurator
{
    public NhibernateConfigurator(string connectionString) : base(shouldMapDtos: false, connectionString: connectionString)
    {
#if DEBUG
        NHibernateProfiler.Initialize();
#endif
    }

    protected override Assembly[] GetAssembliesToMap()
    {
        return
        [
            typeof(Watchdog).Assembly
        ];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
#if DEBUG
            NHibernateProfiler.Shutdown();
#endif
        }
        base.Dispose(disposing);
    }
}
